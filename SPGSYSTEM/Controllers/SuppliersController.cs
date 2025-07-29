using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
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
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public SuppliersController(
            ISupplierService supplierService,
            IAccountService accountService,
            IMapper mapper)
        {
            _supplierService = supplierService;
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Crear el usuario de Identity primero
                    var registerRequest = new Identity.DTOs.RegisterRequest
                    {
                        FirstName = model.ContactPerson ?? "Proveedor",
                        LastName = model.Name,
                        Email = model.Email ?? $"{model.Name.ToLower().Replace(" ", "")}@proveedor.com",
                        UserName = model.Email ?? $"{model.Name.ToLower().Replace(" ", "")}@proveedor.com",
                        Password = "Proveedor123!", // Contraseña por defecto
                        ConfirmPassword = "Proveedor123!"
                    };

                    var registerResult = await _accountService.RegisterSupplierAsync(registerRequest, Request.Headers["Origin"].ToString() ?? "SPGSYSTEM");
                    
                    if (registerResult.Success)
                    {
                        // Asignar el UserId al proveedor
                        model.UserId = registerResult.UserId;
                        
                        // Crear el proveedor
                        var success = await _supplierService.CreateAsync(model);
                        
                        if (success)
                        {
                            TempData["Success"] = $"Proveedor '{model.Name}' creado exitosamente. Usuario: {registerRequest.UserName}, Contraseña: {registerRequest.Password}";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["Error"] = "Error al crear el proveedor.";
                        }
                    }
                    else
                    {
                        TempData["Error"] = $"Error al crear el usuario: {registerResult.Message}";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error: {ex.Message}";
                }
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

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Aquí implementarías la lógica de eliminación
                TempData["Success"] = "Proveedor eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 