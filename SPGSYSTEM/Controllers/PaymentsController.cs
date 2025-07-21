using Application.Interfaces.Services;
using Application.ViewModels.Payment;
using AutoMapper;
using Database.Entities;
using Database.Enum;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public PaymentsController(
            IPaymentService paymentService,
            ISaleService saleService,
            IProductService productService,
            ICustomerService customerService,
            IMapper mapper)
        {
            _paymentService = paymentService;
            _saleService = saleService;
            _productService = productService;
            _customerService = customerService;
            _mapper = mapper;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            try
            {
                var payments = await _paymentService.GetAllWithDetailsAsync();
                var viewModels = _mapper.Map<List<PaymentViewModel>>(payments);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los pagos: " + ex.Message;
                return View(new List<PaymentViewModel>());
            }
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<PaymentViewModel>(payment);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Payments/CreateEdit (para crear)
        public async Task<IActionResult> CreateEdit()
        {
            try
            {
                // Solo ventas sin pago asociado
                var allSales = await _saleService.GetAllAsync();
                var salesWithoutPayment = new List<Sale>();

                foreach (var sale in allSales)
                {
                    var existingPayment = await _paymentService.GetBySaleIdAsync(sale.Id);
                    if (existingPayment == null)
                    {
                        var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                        salesWithoutPayment.Add(fullSale);
                    }
                }

                ViewBag.Sales = salesWithoutPayment;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Pago";

                return View(new PaymentSaveViewModel { PaymentDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario de pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Payments/CreateEdit/5 (para editar)
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var allSales = await _saleService.GetAllAsync();
                var salesWithDetails = new List<Sale>();
                foreach (var sale in allSales)
                {
                    var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                    salesWithDetails.Add(fullSale);
                }

                var viewModel = new PaymentSaveViewModel
                {
                    SaleId = payment.SaleId,
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate
                };

                ViewBag.Sales = salesWithDetails;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Pago";
                ViewBag.PaymentId = id;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payments/CreateEdit (crear)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(PaymentSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if sale already has payment
                    var existingPayment = await _paymentService.GetBySaleIdAsync(model.SaleId);
                    if (existingPayment != null)
                    {
                        TempData["Error"] = "Esta venta ya tiene un pago asociado.";
                        return await ReloadCreateEditView(model, false);
                    }

                    // Get sale details
                    var sale = await _saleService.GetFullSaleAsync(model.SaleId);
                    if (sale == null)
                    {
                        TempData["Error"] = "Venta no encontrada.";
                        return await ReloadCreateEditView(model, false);
                    }

                    // Validate payment amount
                    if (model.Amount != sale.TotalAmount)
                    {
                        TempData["Error"] = $"El monto del pago debe ser igual al total de la venta: ${sale.TotalAmount:N2}";
                        return await ReloadCreateEditView(model, false);
                    }

                    // Create payment
                    var payment = new Payment
                    {
                        SaleId = model.SaleId,
                        PaymentMethod = model.PaymentMethod,
                        Amount = model.Amount,
                        PaymentDate = DateTime.Now
                    };

                    await _paymentService.CreateAsync(payment);

                    // Process stock updates for the sale
                    foreach (var detail in sale.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.Stock -= detail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    TempData["Success"] = "Pago procesado exitosamente. El stock ha sido actualizado.";
                    return RedirectToAction(nameof(Index));
                }

                return await ReloadCreateEditView(model, false);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar el pago: " + ex.Message;
                return await ReloadCreateEditView(model, false);
            }
        }

        // POST: Payments/CreateEdit/5 (editar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(int id, PaymentSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingPayment = await _paymentService.GetWithDetailsAsync(id);
                    if (existingPayment == null)
                    {
                        TempData["Error"] = "Pago no encontrado.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Get sale details
                    var sale = await _saleService.GetFullSaleAsync(model.SaleId);
                    if (sale == null)
                    {
                        TempData["Error"] = "Venta no encontrada.";
                        return await ReloadCreateEditView(model, true, id);
                    }

                    // Validate payment amount
                    if (model.Amount != sale.TotalAmount)
                    {
                        TempData["Error"] = $"El monto del pago debe ser igual al total de la venta: ${sale.TotalAmount:N2}";
                        return await ReloadCreateEditView(model, true, id);
                    }

                    // Update payment
                    existingPayment.PaymentMethod = model.PaymentMethod;
                    existingPayment.Amount = model.Amount;

                    await _paymentService.UpdateAsync(existingPayment);
                    TempData["Success"] = "Pago actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                return await ReloadCreateEditView(model, true, id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el pago: " + ex.Message;
                return await ReloadCreateEditView(model, true, id);
            }
        }

        // GET: Payments/SimulatePayment/5 (simular pago para una venta)
        public async Task<IActionResult> SimulatePayment(int saleId)
        {
            try
            {
                var sale = await _saleService.GetFullSaleAsync(saleId);
                if (sale == null)
                {
                    TempData["Error"] = "Venta no encontrada.";
                    return RedirectToAction("Index", "Sales");
                }

                // Check if payment already exists
                var existingPayment = await _paymentService.GetBySaleIdAsync(saleId);
                if (existingPayment != null)
                {
                    TempData["Error"] = "Esta venta ya tiene un pago procesado.";
                    return RedirectToAction("Details", "Sales", new { id = saleId });
                }

                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.Sale = sale;

                var paymentModel = new PaymentSaveViewModel
                {
                    SaleId = saleId,
                    Amount = sale.TotalAmount,
                    PaymentDate = DateTime.Now
                };

                return View(paymentModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la simulación de pago: " + ex.Message;
                return RedirectToAction("Index", "Sales");
            }
        }

        // POST: Payments/ProcessPayment (procesar pago simulado)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentSaveViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get sale details
                    var sale = await _saleService.GetFullSaleAsync(model.SaleId);
                    if (sale == null)
                    {
                        TempData["Error"] = "Venta no encontrada.";
                        return RedirectToAction("Index", "Sales");
                    }

                    // Check if payment already exists
                    var existingPayment = await _paymentService.GetBySaleIdAsync(model.SaleId);
                    if (existingPayment != null)
                    {
                        TempData["Error"] = "Esta venta ya tiene un pago procesado.";
                        return RedirectToAction("Details", "Sales", new { id = model.SaleId });
                    }

                    // Simulate payment processing delay
                    await Task.Delay(2000); // 2 seconds simulation

                    // Create payment
                    var payment = new Payment
                    {
                        SaleId = model.SaleId,
                        PaymentMethod = model.PaymentMethod,
                        Amount = model.Amount,
                        PaymentDate = DateTime.Now
                    };

                    await _paymentService.CreateAsync(payment);

                    // Update product stock
                    foreach (var detail in sale.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.Stock -= detail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    TempData["Success"] = "¡Pago procesado exitosamente! El stock del inventario ha sido actualizado.";
                    return RedirectToAction("Details", "Sales", new { id = model.SaleId });
                }

                // If validation fails, reload the simulation view
                var sale2 = await _saleService.GetFullSaleAsync(model.SaleId);
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.Sale = sale2;
                return View("SimulatePayment", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar el pago: " + ex.Message;
                return RedirectToAction("Index", "Sales");
            }
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<PaymentViewModel>(payment);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Restore stock when payment is deleted
                var sale = await _saleService.GetFullSaleAsync(payment.SaleId);
                if (sale != null)
                {
                    foreach (var detail in sale.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.Stock += detail.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }
                }

                await _paymentService.DeleteAsync(id);
                TempData["Success"] = "Pago eliminado exitosamente. El stock ha sido restaurado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Search Payments
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var payments = await _paymentService.GetAllWithDetailsAsync();
                var filteredPayments = payments.Where(p => 
                    string.IsNullOrEmpty(searchTerm) || 
                    p.Sale.Customer.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Sale.Id.ToString().Contains(searchTerm) ||
                    p.PaymentMethod.ToString().ToLower().Contains(searchTerm.ToLower())
                ).ToList();

                var viewModels = _mapper.Map<List<PaymentViewModel>>(filteredPayments);
                return Json(viewModels);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Helper method to reload CreateEdit view
        private async Task<IActionResult> ReloadCreateEditView(PaymentSaveViewModel model, bool isEdit, int? id = null)
        {
            if (isEdit)
            {
                var allSales = await _saleService.GetAllAsync();
                var salesWithDetails = new List<Sale>();
                foreach (var sale in allSales)
                {
                    var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                    salesWithDetails.Add(fullSale);
                }
                ViewBag.Sales = salesWithDetails;
            }
            else
            {
                var allSales = await _saleService.GetAllAsync();
                var salesWithoutPayment = new List<Sale>();

                foreach (var sale in allSales)
                {
                    var existingPayment = await _paymentService.GetBySaleIdAsync(sale.Id);
                    if (existingPayment == null)
                    {
                        var fullSale = await _saleService.GetFullSaleAsync(sale.Id);
                        salesWithoutPayment.Add(fullSale);
                    }
                }
                ViewBag.Sales = salesWithoutPayment;
            }

            ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
            ViewBag.IsEdit = isEdit;
            ViewBag.PageTitle = isEdit ? "Editar Pago" : "Nuevo Pago";
            if (id.HasValue) ViewBag.PaymentId = id.Value;

            return View(model);
        }
    }
} 