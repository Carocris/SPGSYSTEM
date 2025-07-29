using Identity.DTOs;
using Identity.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SPGSYSTEM.Controllers
{
    /// <summary>
    /// Controlador para gestión de autenticación
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Muestra la página de login
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Procesa el login del usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthenticationRequest request, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var response = await _accountService.AuthenticateAsync(request);

                if (response.HasError)
                {
                    ModelState.AddModelError(string.Empty, response.Error);
                    return View(request);
                }

                // Crear claims para el usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.Id),
                    new Claim(ClaimTypes.Name, response.UserName),
                    new Claim(ClaimTypes.Email, response.Email),
                    new Claim("FirstName", response.FirstName),
                    new Claim("LastName", response.LastName),
                    new Claim("FullName", $"{response.FirstName} {response.LastName}")
                };

                // Agregar roles como claims
                foreach (var role in response.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Success"] = $"Bienvenido, {response.FirstName} {response.LastName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error durante el login: {ex.Message}");
                return View(request);
            }
        }

        /// <summary>
        /// Muestra la página de registro
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Procesa el registro de un nuevo usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var response = await _accountService.RegisterBasicAsync(request, Request.Host.Value);

                if (response.HasError)
                {
                    ModelState.AddModelError(string.Empty, response.Error);
                    return View(request);
                }

                TempData["Success"] = "Usuario registrado exitosamente. Puede iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error durante el registro: {ex.Message}");
                return View(request);
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            TempData["Success"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Muestra página de acceso denegado
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 