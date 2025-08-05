using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
using Application.ViewModels.Product;
using Application.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Identity.Entities;
using Identity.Interfaces;

namespace SPGSYSTEM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public SuppliersController(
            ISupplierService supplierService,
            IProductService productService,
            ISaleService saleService,
            IAccountService accountService,
            IMapper mapper)
        {
            _supplierService = supplierService;
            _productService = productService;
            _saleService = saleService;
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierService.GetAllViewModelsAsync();
            return View(suppliers);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View(new SupplierSaveViewModel());
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierSaveViewModel model)
        {
            Console.WriteLine($"SuppliersController.Create: Iniciando creación de proveedor '{model.Name}'");
            Console.WriteLine($"SuppliersController.Create: Email proporcionado: '{model.Email}'");
            
            if (ModelState.IsValid)
            {
                Console.WriteLine($"SuppliersController.Create: ModelState es válido");
                try
                {
                    // Crear el usuario de Identity primero
                    var email = model.Email;
                    string userName;
                    
                    // Si no se proporciona email, generar uno automáticamente
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        // Limpiar el nombre de caracteres especiales y espacios
                        var cleanName = model.Name.ToLower()
                            .Replace(" ", "")
                            .Replace("-", "")
                            .Replace("_", "")
                            .Replace(".", "")
                            .Replace(",", "")
                            .Replace("á", "a")
                            .Replace("é", "e")
                            .Replace("í", "i")
                            .Replace("ó", "o")
                            .Replace("ú", "u")
                            .Replace("ñ", "n");
                        
                        email = $"{cleanName}@proveedor.com";
                        userName = cleanName; // Usar el nombre limpio como username
                        
                        // Verificar si el username ya existe y agregar un número si es necesario
                        var originalUserName = userName;
                        var counter = 1;
                        while (await _accountService.GetUserByNameAsync(userName) != null)
                        {
                            userName = $"{originalUserName}{counter}";
                            counter++;
                        }
                        
                        Console.WriteLine($"SuppliersController.Create: Email generado automáticamente: {email}");
                        Console.WriteLine($"SuppliersController.Create: Username final: {userName}");
                    }
                    else
                    {
                        // Extraer el username de la parte antes del @ del email
                        userName = email.Split('@')[0].ToLower();
                        
                        // Verificar si el username ya existe y agregar un número si es necesario
                        var originalUserName = userName;
                        var counter = 1;
                        while (await _accountService.GetUserByNameAsync(userName) != null)
                        {
                            userName = $"{originalUserName}{counter}";
                            counter++;
                        }
                        
                        Console.WriteLine($"SuppliersController.Create: Usando email proporcionado: {email}");
                        Console.WriteLine($"SuppliersController.Create: Username final: {userName}");
                    }
                    
                    var registerRequest = new Identity.DTOs.RegisterRequest
                    {
                        CompanyName = model.Name, // Nombre de la empresa
                        ContactName = model.ContactPerson ?? "Proveedor", // Persona de contacto
                        Email = email,
                        UserName = userName,
                        PhoneNumber = model.Phone ?? "000-000-0000",
                        Password = "Proveedor123!",
                        ConfirmPassword = "Proveedor123!"
                    };

                    Console.WriteLine($"SuppliersController.Create: RegisterRequest creado - Email: {registerRequest.Email}, UserName: {registerRequest.UserName}, CompanyName: {registerRequest.CompanyName}, ContactName: {registerRequest.ContactName}");

                    var registerResult = await _accountService.RegisterSupplierAsync(registerRequest, Request.Headers["Origin"].ToString() ?? "SPGSYSTEM");
                    
                    Console.WriteLine($"SuppliersController.Create: Resultado del registro - Success: {registerResult.Success}, HasError: {registerResult.HasError}, Error: {registerResult.Error}");
                    
                    if (registerResult.Success)
                    {
                        Console.WriteLine($"SuppliersController.Create: Usuario creado exitosamente. UserId: {registerResult.UserId}");
                        
                        // Asignar el UserId al proveedor
                        model.UserId = registerResult.UserId;
                        
                        Console.WriteLine($"SuppliersController.Create: Creando proveedor con UserId: {model.UserId}");
                        
                        // Crear el proveedor
                        var success = await _supplierService.CreateAsync(model);
                        
                        Console.WriteLine($"SuppliersController.Create: Resultado de creación de proveedor: {success}");
                        
                        if (success)
                        {
                            var credentialsMessage = $"✅ Proveedor '{model.Name}' creado exitosamente.\n\n" +
                                                   $"🔐 CREDENCIALES DE ACCESO:\n" +
                                                   $"• Usuario: {userName}\n" +
                                                   $"• Contraseña: {registerRequest.Password}\n" +
                                                   $"• Email: {email}\n\n" +
                                                   $"📋 INFORMACIÓN DEL PROVEEDOR:\n" +
                                                   $"• Empresa: {model.Name}\n" +
                                                   $"• Contacto: {model.ContactPerson}\n\n" +
                                                   $"⚠️ IMPORTANTE: Guarde estas credenciales. El proveedor podrá iniciar sesión inmediatamente.";
                            
                            TempData["Success"] = credentialsMessage;
                            Console.WriteLine($"SuppliersController.Create: Proveedor creado exitosamente");
                            Console.WriteLine($"SuppliersController.Create: Credenciales - Usuario: {userName}, Email: {email}, Password: {registerRequest.Password}");
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["Error"] = "Error al crear el proveedor en la base de datos.";
                            Console.WriteLine($"SuppliersController.Create: Error al crear proveedor en BD");
                        }
                    }
                    else
                    {
                        TempData["Error"] = $"Error al crear el usuario: {registerResult.Error}";
                        Console.WriteLine($"SuppliersController.Create: Error al crear usuario: {registerResult.Error}");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error inesperado: {ex.Message}";
                    Console.WriteLine($"SuppliersController.Create: Excepción: {ex.Message}");
                    Console.WriteLine($"SuppliersController.Create: Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"SuppliersController.Create: ModelState no es válido");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"SuppliersController.Create: Error de validación: {error.ErrorMessage}");
                }
                TempData["Error"] = "Por favor, corrija los errores en el formulario.";
            }

            return View(model);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierService.GetSaveViewModelByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierSaveViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _supplierService.UpdateAsync(model);
                    if (success)
                    {
                        TempData["Success"] = "Proveedor actualizado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Error al actualizar el proveedor.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error: {ex.Message}";
                }
            }
            return View(model);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierService.GetViewModelByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            // Obtener los productos específicos de este proveedor
            var supplierProducts = await _productService.GetBySupplierAsync(id);
            var productViewModels = _mapper.Map<List<ProductViewModel>>(supplierProducts);
            ViewBag.SupplierProducts = productViewModels;

            // Obtener las ventas del proveedor
            var supplierSales = await _saleService.GetSalesBySupplierAsync(id);
            var saleViewModels = _mapper.Map<List<SaleViewModel>>(supplierSales);
            ViewBag.SupplierSales = saleViewModels;

            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierService.GetViewModelByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // GET: Suppliers/Credentials/5
        public async Task<IActionResult> Credentials(int id)
        {
            var supplier = await _supplierService.GetViewModelByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            // Obtener información del usuario si existe
            if (!string.IsNullOrEmpty(supplier.UserId))
            {
                var user = await _accountService.GetUserByIdAsync(supplier.UserId);
                if (user != null)
                {
                    ViewBag.UserCredentials = new
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        Message = "Estas son las credenciales del proveedor para acceder al sistema."
                    };
                }
                else
                {
                    ViewBag.UserCredentials = new
                    {
                        UserName = "No disponible",
                        Email = "No disponible",
                        Message = "El proveedor no tiene credenciales de acceso configuradas."
                    };
                }
            }
            else
            {
                ViewBag.UserCredentials = new
                {
                    UserName = "No disponible",
                    Email = "No disponible",
                    Message = "El proveedor no tiene credenciales de acceso configuradas."
                };
            }

            return View(supplier);
        }



        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine($"SuppliersController.DeleteConfirmed: Intentando eliminar proveedor con ID: {id}");
                
                // Obtener el proveedor para verificar que existe
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    Console.WriteLine($"SuppliersController.DeleteConfirmed: Proveedor con ID {id} no encontrado");
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"SuppliersController.DeleteConfirmed: Proveedor encontrado - Nombre: {supplier.Name}, UserId: {supplier.UserId}");

                // Eliminar el proveedor
                var success = await _supplierService.DeleteSupplierAsync(id);
                if (!success)
                {
                    TempData["Error"] = "No se puede eliminar el proveedor porque tiene productos asociados. Primero debe eliminar o reasignar todos los productos de este proveedor.";
                    Console.WriteLine($"SuppliersController.DeleteConfirmed: Error al eliminar proveedor de BD - tiene productos asociados");
                    return RedirectToAction(nameof(Index));
                }
                Console.WriteLine($"SuppliersController.DeleteConfirmed: Proveedor eliminado de BD exitosamente");

                // Si el proveedor tenía un UserId, también eliminar el usuario de Identity
                if (!string.IsNullOrEmpty(supplier.UserId))
                {
                    Console.WriteLine($"SuppliersController.DeleteConfirmed: Eliminando usuario de Identity con ID: {supplier.UserId}");
                    
                    var user = await _accountService.GetUserByIdAsync(supplier.UserId);
                    if (user != null)
                    {
                        // Aquí podrías implementar la eliminación del usuario de Identity
                        // Por ahora solo lo marcamos como inactivo o lo eliminamos lógicamente
                        Console.WriteLine($"SuppliersController.DeleteConfirmed: Usuario de Identity encontrado - UserName: {user.UserName}");
                    }
                    else
                    {
                        Console.WriteLine($"SuppliersController.DeleteConfirmed: Usuario de Identity no encontrado");
                    }
                }

                TempData["Success"] = $"Proveedor '{supplier.Name}' eliminado exitosamente.";
                Console.WriteLine($"SuppliersController.DeleteConfirmed: Proveedor eliminado exitosamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                Console.WriteLine($"SuppliersController.DeleteConfirmed: Excepción - {ex.Message}");
                Console.WriteLine($"SuppliersController.DeleteConfirmed: Stack trace - {ex.StackTrace}");
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 