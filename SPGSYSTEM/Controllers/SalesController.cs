using Application.Interfaces.Services;
using Application.ViewModels.Sale;
using Application.ViewModels.SaleDetail;
using Application.ViewModels.Payment;
using AutoMapper;
using Database.Entities;
using Database.Enum;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class SalesController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public SalesController(
            ISaleService saleService,
            ICustomerService customerService,
            IProductService productService,
            IPaymentService paymentService,
            IMapper mapper)
        {
            _saleService = saleService;
            _customerService = customerService;
            _productService = productService;
            _paymentService = paymentService;
            _mapper = mapper;
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            try
            {
                var sales = await _saleService.GetAllAsync();
                var viewModels = new List<SaleViewModel>();

                foreach (var sale in sales)
                {
                    var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                    var viewModel = _mapper.Map<SaleViewModel>(fullSale);
                    viewModels.Add(viewModel);
                }

                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las ventas: " + ex.Message;
                return View(new List<SaleViewModel>());
            }
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sale = await _saleService.GetFullSaleAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Venta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SaleViewModel>(sale);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de la venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sales/CreateEdit (para crear)
        public async Task<IActionResult> CreateEdit()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nueva Venta";

                return View(new SaleSaveViewModel { Details = new List<SaleDetailSaveViewModel>() });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario de venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sales/CreateEdit/5 (para editar)
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var sale = await _saleService.GetFullSaleAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Venta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                // Map sale to SaleSaveViewModel
                var viewModel = new SaleSaveViewModel
                {
                    CustomerId = sale.CustomerId,
                    PaymentMethod = sale.Payment.PaymentMethod,
                    AmountPaid = sale.TotalAmount,
                    Details = sale.Details.Select(d => new SaleDetailSaveViewModel
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity
                    }).ToList()
                };

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Venta";
                ViewBag.SaleId = id;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sales/CreateEdit (crear)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(SaleSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Calculate totals
                    decimal totalAmount = 0;
                    var products = await _productService.GetAllAsync();

                    foreach (var detail in model.Details)
                    {
                        var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                        if (product != null)
                        {
                            detail.Subtotal = product.Price * detail.Quantity;
                            totalAmount += detail.Subtotal;
                        }
                    }

                    // Create Sale entity
                    var sale = new Sale
                    {
                        CustomerId = model.CustomerId,
                        SaleDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Details = model.Details.Select(d => new SaleDetail
                        {
                            ProductId = d.ProductId,
                            Quantity = d.Quantity,
                            Subtotal = d.Subtotal
                        }).ToList(),
                        Payment = new Payment
                        {
                            PaymentMethod = model.PaymentMethod,
                            Amount = model.AmountPaid,
                            PaymentDate = DateTime.Now
                        }
                    };

                    // Update product stock
                    foreach (var detail in model.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.Stock -= detail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    await _saleService.CreateSaleAsync(sale);
                    TempData["Success"] = "Venta creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed, redisplay form
                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nueva Venta";

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la venta: " + ex.Message;

                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nueva Venta";

                return View(model);
            }
        }

        // POST: Sales/CreateEdit/5 (editar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(int id, SaleSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingSale = await _saleService.GetFullSaleAsync(id);
                    if (existingSale == null)
                    {
                        TempData["Error"] = "Venta no encontrada.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Restore stock from original sale
                    foreach (var originalDetail in existingSale.Details)
                    {
                        var product = await _productService.GetByIdAsync(originalDetail.ProductId);
                        if (product != null)
                        {
                            product.Stock += originalDetail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    // Calculate new totals
                    decimal totalAmount = 0;
                    var products = await _productService.GetAllAsync();

                    foreach (var detail in model.Details)
                    {
                        var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                        if (product != null)
                        {
                            detail.Subtotal = product.Price * detail.Quantity;
                            totalAmount += detail.Subtotal;
                        }
                    }

                    // Update sale
                    existingSale.TotalAmount = totalAmount;
                    existingSale.Payment.PaymentMethod = model.PaymentMethod;
                    existingSale.Payment.Amount = model.AmountPaid;

                    // Clear existing details and add new ones
                    existingSale.Details.Clear();
                    existingSale.Details = model.Details.Select(d => new SaleDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        Subtotal = d.Subtotal,
                        SaleId = id
                    }).ToList();

                    // Update product stock with new quantities
                    foreach (var detail in model.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.Stock -= detail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    await _saleService.UpdateAsync(existingSale);
                    TempData["Success"] = "Venta actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed, redisplay form
                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Venta";
                ViewBag.SaleId = id;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la venta: " + ex.Message;

                var customers = await _customerService.GetAllAsync();
                var products = await _productService.GetAllAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Venta";
                ViewBag.SaleId = id;

                return View(model);
            }
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sale = await _saleService.GetFullSaleAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Venta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SaleViewModel>(sale);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sale = await _saleService.GetFullSaleAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Venta no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Restore product stock
                foreach (var detail in sale.Details)
                {
                    var product = await _productService.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                        await _productService.UpdateAsync(product);
                    }
                }

                await _saleService.DeleteAsync(id);
                TempData["Success"] = "Venta eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la venta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Search Sales
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var sales = await _saleService.GetAllAsync();
                var filteredSales = sales.Where(s => 
                    string.IsNullOrEmpty(searchTerm) || 
                    s.Id.ToString().Contains(searchTerm) ||
                    s.Customer.Name.ToLower().Contains(searchTerm.ToLower())
                ).ToList();

                var viewModels = new List<SaleViewModel>();
                foreach (var sale in filteredSales)
                {
                    var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                    var viewModel = _mapper.Map<SaleViewModel>(fullSale);
                    viewModels.Add(viewModel);
                }

                return Json(viewModels);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
} 