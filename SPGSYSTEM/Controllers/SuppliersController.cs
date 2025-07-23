using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
using Application.ViewModels.SupplierPrice;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ISupplierPriceService _supplierPriceService;
        private readonly IMapper _mapper;

        public SuppliersController(ISupplierService supplierService, ISupplierPriceService supplierPriceService, IMapper mapper)
        {
            _supplierService = supplierService;
            _supplierPriceService = supplierPriceService;
            _mapper = mapper;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllWithProductsAsync();
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
                
                // Cargar los productos del proveedor para mostrarlos en la vista
                if (supplier.Products != null)
                {
                    ViewBag.SupplierProducts = supplier.Products.Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description,
                        Stock = p.Stock,
                        PurchasePrice = p.PurchasePrice,
                        SalePrice = p.SalePrice,
                        CategoryName = p.Category?.Name ?? "Sin categoría"
                    }).ToList();
                }
                else
                {
                    ViewBag.SupplierProducts = new List<object>();
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Nuevo Proveedor";
            return View("CreateEdit", new SupplierSaveViewModel());
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int id)
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
                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Proveedor";
                return View("CreateEdit", viewModel);
            }

            try
            {
                // Verificar si ya existe un proveedor con ese nombre
                if (await _supplierService.ExistsAsync(viewModel.Name))
                {
                    ModelState.AddModelError("Name", "Ya existe un proveedor con este nombre.");
                    ViewBag.IsEdit = false;
                    ViewBag.PageTitle = "Nuevo Proveedor";
                    return View("CreateEdit", viewModel);
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
                return View("CreateEdit", viewModel);
            }
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Proveedor";
                ViewBag.SupplierId = id;
                return View("CreateEdit", viewModel);
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
                    return View("CreateEdit", viewModel);
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
                return View("CreateEdit", viewModel);
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

        // GET: Suppliers/SupplierProducts/5
        public async Task<IActionResult> SupplierProducts(int id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var supplierPrices = await _supplierPriceService.GetBySupplierAsync(id);
                var viewModels = _mapper.Map<List<SupplierPriceViewModel>>(supplierPrices);
                
                var supplierViewModel = _mapper.Map<SupplierViewModel>(supplier);
                
                ViewBag.SupplierPrices = viewModels;
                ViewBag.AveragePrice = viewModels.Any() ? viewModels.Average(sp => sp.Price) : 0;
                ViewBag.HighestPrice = viewModels.Any() ? viewModels.Max(sp => sp.Price) : 0;
                
                return View(supplierViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los productos del proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 