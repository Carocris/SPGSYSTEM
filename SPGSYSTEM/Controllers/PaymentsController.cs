using Application.Interfaces.Services;
using Application.ViewModels.Payment;
using AutoMapper;
using Database.Entities;
using Database.Enum;
using Microsoft.AspNetCore.Mvc;
using SPGSYSTEM.Helpers;

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

        // GET: Payments/CreateEdit
        public async Task<IActionResult> CreateEdit(int? id = null)
        {
            try
            {
                if (id.HasValue)
                {
                    // EDITAR PAGO EXISTENTE
                    var payment = await _paymentService.GetWithDetailsAsync(id.Value);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                    var model = _mapper.Map<PaymentSaveViewModel>(payment);
                    
                    // Solo mostrar la venta asociada
                    var sale = await _saleService.GetFullSaleAsync(payment.SaleId);
                    ViewBag.Sales = new List<Sale> { sale };
                    ViewBag.PaymentMethods = Enum.GetValues<PaymentMethodType>();
                    ViewBag.PaymentStatuses = Enum.GetValues<PaymentStatusType>();
                ViewBag.IsEdit = true;
                    ViewBag.PageTitle = "Gestionar Pago";
                ViewBag.PaymentId = id;

                    return View(model);
                }
                else
                {
                    // REDIRIGIR A VENTAS PARA CREAR NUEVO PAGO
                    TempData["Info"] = "Para crear un nuevo pago, ve a Ventas y crea una nueva venta con pago incluido.";
                    return RedirectToAction("CreateEdit", "Sales");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payments/CreateEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(PaymentSaveViewModel model, int? id = null)
        {
            try
            {
                if (!id.HasValue)
                {
                    // REDIRIGIR A VENTAS PARA CREAR NUEVO PAGO
                    TempData["Info"] = "Para crear un nuevo pago, ve a Ventas y crea una nueva venta con pago incluido.";
                    return RedirectToAction("CreateEdit", "Sales");
                }

                if (!ModelState.IsValid)
                {
                    await ReloadCreateEditView(id.Value);
                    return View(model);
                    }

                // EDITAR PAGO EXISTENTE
                var existingPayment = await _paymentService.GetWithDetailsAsync(id.Value);
                if (existingPayment == null)
                    {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                    }

                // Validar que el monto no sea mayor al de la venta
                var sale = await _saleService.GetFullSaleAsync(model.SaleId);
                if (model.Amount > sale.TotalAmount)
                    {
                    ModelState.AddModelError("Amount", "El monto del pago no puede ser mayor al total de la venta.");
                    await ReloadCreateEditView(id.Value);
                    return View(model);
                    }

                // Calcular diferencia de monto para validar stock
                var amountDifference = Math.Abs(model.Amount - existingPayment.Amount);
                var tolerance = 0.01m; // Tolerancia de 1 centavo

                // Solo validar stock si el monto cambió significativamente
                if (amountDifference > tolerance)
                {
                    var stockValidationErrors = new List<string>();
                    foreach (var detail in sale.Details)
                    {
                        var product = await _productService.GetByIdAsync(detail.ProductId);
                        if (product.Stock < detail.Quantity)
                        {
                            stockValidationErrors.Add($"Stock insuficiente para {product.Name}: {product.Stock} disponibles, {detail.Quantity} requeridos");
                        }
                    }

                    if (stockValidationErrors.Any())
                    {
                        ModelState.AddModelError("", "Stock insuficiente para procesar el pago:");
                        foreach (var error in stockValidationErrors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        await ReloadCreateEditView(id.Value);
                        return View(model);
                        }
                    }

                // Actualizar pago existente
                existingPayment.PaymentMethod = model.PaymentMethod;
                existingPayment.Amount = model.Amount;
                existingPayment.PaymentDate = model.PaymentDate;
                existingPayment.Status = model.Status;

                await _paymentService.UpdateAsync(existingPayment);

                // Manejar actualización de stock según el estado
                await HandleStockUpdate(existingPayment, sale);

                TempData["Success"] = "Pago actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el pago: " + ex.Message;
                if (id.HasValue)
                {
                    await ReloadCreateEditView(id.Value);
                }
                return View(model);
            }
        }

        // POST: Payments/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, PaymentStatusType newStatus)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                    {
                        TempData["Error"] = "Pago no encontrado.";
                        return RedirectToAction(nameof(Index));
                    }

                var oldStatus = payment.Status;
                payment.Status = newStatus;

                // Obtener la venta asociada
                var sale = await _saleService.GetFullSaleAsync(payment.SaleId);

                // Manejar actualización de stock según el cambio de estado
                if (oldStatus != newStatus)
                    {
                    await HandleStockUpdate(payment, sale);
                }

                await _paymentService.UpdateAsync(payment);

                var statusMessage = newStatus == PaymentStatusType.Completed ? "completado" : "cancelado";
                TempData["Success"] = $"Pago {statusMessage} exitosamente.";
                    return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cambiar el estado del pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
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

        // GET: Payments/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (payment.Status == PaymentStatusType.Cancelled)
                {
                    TempData["Warning"] = "Este pago ya está cancelado.";
                    return RedirectToAction(nameof(Index));
                }

                // Cambiar estado a cancelado
                payment.Status = PaymentStatusType.Cancelled;

                // Obtener la venta asociada para restaurar stock
                var sale = await _saleService.GetFullSaleAsync(payment.SaleId);
                if (sale != null)
                {
                    // Restaurar stock de productos
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

                await _paymentService.UpdateAsync(payment);

                TempData["Success"] = $"Pago #{payment.Id:D4} cancelado exitosamente. Stock restaurado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cancelar el pago: " + ex.Message;
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
        private async Task ReloadCreateEditView(int id)
        {
            try
            {
                var payment = await _paymentService.GetWithDetailsAsync(id);
                if (payment != null)
                {
                    var sale = await _saleService.GetFullSaleAsync(payment.SaleId);
                    ViewBag.Sales = new List<Sale> { sale };
                    ViewBag.PaymentMethods = Enum.GetValues<PaymentMethodType>();
                    ViewBag.PaymentStatuses = Enum.GetValues<PaymentStatusType>();
                    ViewBag.IsEdit = true;
                    ViewBag.PageTitle = "Gestionar Pago";
                    ViewBag.PaymentId = id;
            }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al recargar la vista: " + ex.Message;
            }
        }

        private string GetPaymentMethodDisplayName(PaymentMethodType method)
        {
            return method switch
            {
                PaymentMethodType.Cash => "💵 Efectivo",
                PaymentMethodType.Card => "💳 Tarjeta",
                PaymentMethodType.Transfer => "🏦 Transferencia",
                _ => method.ToString()
            };
        }

        private async Task HandleStockUpdate(Payment payment, Sale sale)
        {
            try
            {
                foreach (var detail in sale.Details)
                {
                    var product = await _productService.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        if (payment.Status == PaymentStatusType.Completed)
                    {
                            // Reducir stock cuando el pago se completa
                            if (product.Stock >= detail.Quantity)
                            {
                                product.Stock -= detail.Quantity;
                                await _productService.UpdateAsync(product);
                            }
                        }
                        else if (payment.Status == PaymentStatusType.Cancelled)
                        {
                            // Restaurar stock cuando el pago se cancela
                            product.Stock += detail.Quantity;
                            await _productService.UpdateAsync(product);
            }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to avoid breaking the payment process
                Console.WriteLine($"Error updating stock for payment {payment.Id}: {ex.Message}");
            }
        }

        private string GetPaymentStatusDisplayName(PaymentStatusType status)
        {
            return status switch
            {
                PaymentStatusType.Completed => "Completado",
                PaymentStatusType.Cancelled => "Cancelado",
                _ => status.ToString()
            };
        }
    }
} 