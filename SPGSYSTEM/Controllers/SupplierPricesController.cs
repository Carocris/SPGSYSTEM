using Application.Interfaces.Services;
using Application.ViewModels.SupplierPrice;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SPGSYSTEM.Controllers
{
    public class SupplierPricesController : Controller
    {
        private readonly ISupplierPriceService _supplierPriceService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public SupplierPricesController(
            ISupplierPriceService supplierPriceService,
            ISupplierService supplierService,
            IProductService productService,
            IMapper mapper)
        {
            _supplierPriceService = supplierPriceService;
            _supplierService = supplierService;
            _productService = productService;
            _mapper = mapper;
        }

        private async Task LoadViewBagData()
        {
            var suppliers = await _supplierService.GetActiveAsync();
            var products = await _productService.GetAllAsync();

            ViewBag.Suppliers = suppliers.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            ViewBag.Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Code} - {p.Name}" }).ToList();
        }

        // GET: SupplierPrices
        public async Task<IActionResult> Index()
        {
            try
            {
                var supplierPrices = await _supplierPriceService.GetActiveAsync();
                var viewModels = _mapper.Map<List<SupplierPriceViewModel>>(supplierPrices);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los precios de proveedores: " + ex.Message;
                return View(new List<SupplierPriceViewModel>());
            }
        }

        // GET: SupplierPrices/Create
        public async Task<IActionResult> Create(int? supplierId = null)
        {
            try
            {
                await LoadViewBagData();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Precio de Proveedor";
                
                var viewModel = new SupplierPriceSaveViewModel();
                if (supplierId.HasValue)
                {
                    viewModel.SupplierId = supplierId.Value;
                }
                
                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: SupplierPrices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierPriceSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBagData();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Precio de Proveedor";
                return View("CreateEdit", viewModel);
            }

            try
            {
                // Verificar si ya existe un precio para esta combinación
                var existingPrice = await _supplierPriceService.GetBySupplierAndProductAsync(viewModel.SupplierId, viewModel.ProductId);
                if (existingPrice != null)
                {
                    ModelState.AddModelError("", "Ya existe un precio para esta combinación de proveedor y producto.");
                    
                    await LoadViewBagData();
                    ViewBag.IsEdit = false;
                    ViewBag.PageTitle = "Nuevo Precio de Proveedor";
                    return View("CreateEdit", viewModel);
                }

                var supplierPrice = _mapper.Map<SupplierPrice>(viewModel);
                await _supplierPriceService.CreateAsync(supplierPrice);
                
                TempData["Success"] = $"Precio de proveedor creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el precio de proveedor: " + ex.Message;
                
                await LoadViewBagData();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Precio de Proveedor";
                return View("CreateEdit", viewModel);
            }
        }

        // GET: SupplierPrices/BySupplier/5
        public async Task<IActionResult> BySupplier(int id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Index", "Suppliers");
                }

                var supplierPrices = await _supplierPriceService.GetBySupplierAsync(id);
                var viewModels = _mapper.Map<List<SupplierPriceViewModel>>(supplierPrices);
                
                ViewBag.SupplierName = supplier.Name;
                ViewBag.SupplierId = id;
                
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los precios del proveedor: " + ex.Message;
                return RedirectToAction("Index", "Suppliers");
            }
        }

        // GET: SupplierPrices/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplierPrice = await _supplierPriceService.GetByIdAsync(id);
                if (supplierPrice == null)
                {
                    TempData["Error"] = "Precio de proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SupplierPriceSaveViewModel>(supplierPrice);
                
                await LoadViewBagData();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Precio de Proveedor";
                
                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el precio de proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: SupplierPrices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierPriceSaveViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID no válido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadViewBagData();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Precio de Proveedor";
                return View("CreateEdit", viewModel);
            }

            try
            {
                var supplierPrice = _mapper.Map<SupplierPrice>(viewModel);
                supplierPrice.LastUpdated = DateTime.Now;
                await _supplierPriceService.UpdateAsync(supplierPrice);
                
                TempData["Success"] = "Precio de proveedor actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el precio de proveedor: " + ex.Message;
                
                await LoadViewBagData();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Precio de Proveedor";
                return View("CreateEdit", viewModel);
            }
        }

        // GET: SupplierPrices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplierPrice = await _supplierPriceService.GetByIdAsync(id);
                if (supplierPrice == null)
                {
                    TempData["Error"] = "Precio de proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SupplierPriceViewModel>(supplierPrice);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles del precio de proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: SupplierPrices/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplierPrice = await _supplierPriceService.GetByIdAsync(id);
                if (supplierPrice == null)
                {
                    TempData["Error"] = "Precio de proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                await _supplierPriceService.DeleteAsync(id);
                TempData["Success"] = "Precio de proveedor eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el precio de proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: SupplierPrices/ByProduct/5
        public async Task<IActionResult> ByProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction("Index", "Products");
                }

                var supplierPrices = await _supplierPriceService.GetByProductAsync(id);
                var viewModels = _mapper.Map<List<SupplierPriceViewModel>>(supplierPrices);
                
                ViewBag.ProductName = product.Name;
                ViewBag.ProductId = id;
                
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los precios del producto: " + ex.Message;
                return RedirectToAction("Index", "Products");
            }
        }
    }
} 