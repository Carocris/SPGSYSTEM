using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public SuppliersController(ISupplierService supplierService, IMapper mapper)
        {
            _supplierService = supplierService;
            _mapper = mapper;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync();
                var viewModels = _mapper.Map<List<SupplierViewModel>>(suppliers);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los proveedores: " + ex.Message;
                return View(new List<SupplierViewModel>());
            }
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplier = await _supplierService.GetWithProductsAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SupplierViewModel>(supplier);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/CreateEdit
        public IActionResult CreateEdit()
        {
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Nuevo Proveedor";
            return View(new SupplierSaveViewModel());
        }

        // GET: Suppliers/CreateEdit/5
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SupplierSaveViewModel>(supplier);
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Proveedor";
                ViewBag.SupplierId = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/CreateEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(SupplierSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Proveedor";
                return View(viewModel);
            }

            try
            {
                // Verificar si ya existe un proveedor con ese nombre
                if (await _supplierService.ExistsAsync(viewModel.Name))
                {
                    ModelState.AddModelError("Name", "Ya existe un proveedor con este nombre.");
                    ViewBag.IsEdit = false;
                    ViewBag.PageTitle = "Nuevo Proveedor";
                    return View(viewModel);
                }

                var supplier = _mapper.Map<Supplier>(viewModel);
                await _supplierService.CreateAsync(supplier);
                
                TempData["Success"] = $"Proveedor '{supplier.Name}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el proveedor: " + ex.Message;
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Proveedor";
                return View(viewModel);
            }
        }

        // POST: Suppliers/CreateEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(int id, SupplierSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Proveedor";
                ViewBag.SupplierId = id;
                return View(viewModel);
            }

            try
            {
                var existingSupplier = await _supplierService.GetByIdAsync(id);
                if (existingSupplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si ya existe otro proveedor con ese nombre
                if (await _supplierService.ExistsAsync(viewModel.Name, id))
                {
                    ModelState.AddModelError("Name", "Ya existe otro proveedor con este nombre.");
                    ViewBag.IsEdit = true;
                    ViewBag.PageTitle = "Editar Proveedor";
                    ViewBag.SupplierId = id;
                    return View(viewModel);
                }

                // Mapear los cambios
                _mapper.Map(viewModel, existingSupplier);
                await _supplierService.UpdateAsync(existingSupplier);
                
                TempData["Success"] = $"Proveedor '{existingSupplier.Name}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el proveedor: " + ex.Message;
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Proveedor";
                ViewBag.SupplierId = id;
                return View(viewModel);
            }
        }

        // POST: Suppliers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplier = await _supplierService.GetWithProductsAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si el proveedor tiene productos asociados
                if (supplier.Products?.Any() == true)
                {
                    TempData["Error"] = $"No se puede eliminar el proveedor '{supplier.Name}' porque tiene productos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                await _supplierService.DeleteAsync(id);
                TempData["Success"] = $"Proveedor '{supplier.Name}' eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el proveedor: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Get active suppliers (for dropdowns)
        [HttpGet]
        public async Task<JsonResult> GetActive()
        {
            try
            {
                var suppliers = await _supplierService.GetActiveAsync();
                var result = suppliers.Select(s => new { id = s.Id, name = s.Name }).ToList();
                return Json(result);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }
} 