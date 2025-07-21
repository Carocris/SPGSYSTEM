using Application.Interfaces.Services;
using Application.ViewModels.SaleDetail;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class SaleDetailsController : Controller
    {
        private readonly ISaleDetailService _saleDetailService;
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public SaleDetailsController(
            ISaleDetailService saleDetailService,
            ISaleService saleService,
            IProductService productService,
            ICustomerService customerService,
            IMapper mapper)
        {
            _saleDetailService = saleDetailService;
            _saleService = saleService;
            _productService = productService;
            _customerService = customerService;
            _mapper = mapper;
        }

        // GET: SaleDetails
        public async Task<IActionResult> Index()
        {
            try
            {
                var saleDetails = await _saleDetailService.GetAllWithDetailsAsync();
                var viewModels = _mapper.Map<List<SaleDetailViewModel>>(saleDetails);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de venta: " + ex.Message;
                return View(new List<SaleDetailViewModel>());
            }
        }

        // GET: SaleDetails/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var saleDetail = await _saleDetailService.GetWithDetailsAsync(id);
                if (saleDetail == null)
                {
                    TempData["Error"] = "Detalle de venta no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SaleDetailViewModel>(saleDetail);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el detalle de venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: SaleDetails/CreateEdit (para crear)
        public async Task<IActionResult> CreateEdit()
        {
            try
            {
                var sales = await _saleService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Sales = sales;
                ViewBag.Products = products;
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Detalle de Venta";

                return View(new SaleDetailSaveViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: SaleDetails/CreateEdit/5 (para editar)
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var saleDetail = await _saleDetailService.GetWithDetailsAsync(id);
                if (saleDetail == null)
                {
                    TempData["Error"] = "Detalle de venta no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var sales = await _saleService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                var viewModel = new SaleDetailSaveViewModel
                {
                    SaleId = saleDetail.SaleId,
                    ProductId = saleDetail.ProductId,
                    Quantity = saleDetail.Quantity,
                    Subtotal = saleDetail.Subtotal
                };

                ViewBag.Sales = sales;
                ViewBag.Products = products;
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Detalle de Venta";
                ViewBag.SaleDetailId = id;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el detalle de venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: SaleDetails/CreateEdit (crear)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(SaleDetailSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get product to calculate subtotal
                    var product = await _productService.GetByIdAsync(model.ProductId);
                    if (product == null)
                    {
                        TempData["Error"] = "Producto no encontrado.";
                        return await ReloadCreateEditView(model, false);
                    }

                    // Check stock availability
                    if (product.Stock < model.Quantity)
                    {
                        TempData["Error"] = $"Stock insuficiente. Stock disponible: {product.Stock}";
                        return await ReloadCreateEditView(model, false);
                    }

                    // Calculate subtotal
                    model.Subtotal = product.Price * model.Quantity;

                    // Create SaleDetail entity
                    var saleDetail = _mapper.Map<SaleDetail>(model);

                    // Update product stock
                    product.Stock -= model.Quantity;
                    await _productService.UpdateAsync(product);

                    // Update sale total
                    var sale = await _saleService.GetFullSaleAsync(model.SaleId);
                    if (sale != null)
                    {
                        sale.TotalAmount += model.Subtotal;
                        await _saleService.UpdateAsync(sale);
                    }

                    await _saleDetailService.CreateAsync(saleDetail);
                    TempData["Success"] = "Detalle de venta creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                return await ReloadCreateEditView(model, false);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el detalle de venta: " + ex.Message;
                return await ReloadCreateEditView(model, false);
            }
        }

        // POST: SaleDetails/CreateEdit/5 (editar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(int id, SaleDetailSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingSaleDetail = await _saleDetailService.GetWithDetailsAsync(id);
                    if (existingSaleDetail == null)
                    {
                        TempData["Error"] = "Detalle de venta no encontrado.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Get product to calculate subtotal
                    var product = await _productService.GetByIdAsync(model.ProductId);
                    if (product == null)
                    {
                        TempData["Error"] = "Producto no encontrado.";
                        return await ReloadCreateEditView(model, true, id);
                    }

                    // Restore stock from original quantity
                    var originalProduct = await _productService.GetByIdAsync(existingSaleDetail.ProductId);
                    if (originalProduct != null)
                    {
                        originalProduct.Stock += existingSaleDetail.Quantity;
                        await _productService.UpdateAsync(originalProduct);
                    }

                    // Check new stock availability
                    var newProduct = await _productService.GetByIdAsync(model.ProductId);
                    if (newProduct != null && newProduct.Stock < model.Quantity)
                    {
                        TempData["Error"] = $"Stock insuficiente. Stock disponible: {newProduct.Stock}";
                        
                        // Restore original stock
                        if (originalProduct != null)
                        {
                            originalProduct.Stock -= existingSaleDetail.Quantity;
                            await _productService.UpdateAsync(originalProduct);
                        }
                        
                        return await ReloadCreateEditView(model, true, id);
                    }

                    // Calculate new subtotal
                    var oldSubtotal = existingSaleDetail.Subtotal;
                    model.Subtotal = newProduct.Price * model.Quantity;

                    // Update SaleDetail
                    existingSaleDetail.ProductId = model.ProductId;
                    existingSaleDetail.Quantity = model.Quantity;
                    existingSaleDetail.Subtotal = model.Subtotal;

                    // Update new product stock
                    newProduct.Stock -= model.Quantity;
                    await _productService.UpdateAsync(newProduct);

                    // Update sale total
                    var sale = await _saleService.GetFullSaleAsync(existingSaleDetail.SaleId);
                    if (sale != null)
                    {
                        sale.TotalAmount = sale.TotalAmount - oldSubtotal + model.Subtotal;
                        await _saleService.UpdateAsync(sale);
                    }

                    await _saleDetailService.UpdateAsync(existingSaleDetail);
                    TempData["Success"] = "Detalle de venta actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                return await ReloadCreateEditView(model, true, id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el detalle de venta: " + ex.Message;
                return await ReloadCreateEditView(model, true, id);
            }
        }

        // GET: SaleDetails/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var saleDetail = await _saleDetailService.GetWithDetailsAsync(id);
                if (saleDetail == null)
                {
                    TempData["Error"] = "Detalle de venta no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SaleDetailViewModel>(saleDetail);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el detalle de venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: SaleDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var saleDetail = await _saleDetailService.GetWithDetailsAsync(id);
                if (saleDetail == null)
                {
                    TempData["Error"] = "Detalle de venta no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Restore product stock
                var product = await _productService.GetByIdAsync(saleDetail.ProductId);
                if (product != null)
                {
                    product.Stock += saleDetail.Quantity;
                    await _productService.UpdateAsync(product);
                }

                // Update sale total
                var sale = await _saleService.GetFullSaleAsync(saleDetail.SaleId);
                if (sale != null)
                {
                    sale.TotalAmount -= saleDetail.Subtotal;
                    await _saleService.UpdateAsync(sale);
                }

                await _saleDetailService.DeleteAsync(id);
                TempData["Success"] = "Detalle de venta eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el detalle de venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Search SaleDetails
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var saleDetails = await _saleDetailService.GetAllWithDetailsAsync();
                var filteredSaleDetails = saleDetails.Where(sd => 
                    string.IsNullOrEmpty(searchTerm) || 
                    sd.Product.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    sd.Sale.Customer.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    sd.Sale.Id.ToString().Contains(searchTerm)
                ).ToList();

                var viewModels = _mapper.Map<List<SaleDetailViewModel>>(filteredSaleDetails);
                return Json(viewModels);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Helper method to reload CreateEdit view
        private async Task<IActionResult> ReloadCreateEditView(SaleDetailSaveViewModel model, bool isEdit, int? id = null)
        {
            var sales = await _saleService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            ViewBag.Sales = sales;
            ViewBag.Products = products;
            ViewBag.IsEdit = isEdit;
            ViewBag.PageTitle = isEdit ? "Editar Detalle de Venta" : "Nuevo Detalle de Venta";
            if (id.HasValue) ViewBag.SaleDetailId = id.Value;

            return View(model);
        }
    }
} 