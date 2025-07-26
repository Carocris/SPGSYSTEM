using Application.Interfaces.Services;
using Application.ViewModels.Sale;
using Application.ViewModels.SaleDetail;
using Application.ViewModels.Payment;
using AutoMapper;
using Database.Entities;
using Database.Enum;
using Microsoft.AspNetCore.Mvc;
using SPGSYSTEM.Helpers;

namespace SPGSYSTEM.Controllers
{
    public class SalesController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ISaleDetailService _saleDetailService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SalesController(
            ISaleService saleService,
            ICustomerService customerService,
            IProductService productService,
            IPaymentService paymentService,
            ISaleDetailService saleDetailService,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment)
        {
            _saleService = saleService;
            _customerService = customerService;
            _productService = productService;
            _paymentService = paymentService;
            _saleDetailService = saleDetailService;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper method to get products with stock available for sale
        private async Task<IReadOnlyList<Product>> GetAvailableProductsAsync()
        {
            var allProducts = await _productService.GetAllAsync();
            return allProducts.Where(p => p.Stock > 0).ToList();
        }

        // Helper method to get products with stock + existing products in a sale (for editing)
        private async Task<IReadOnlyList<Product>> GetProductsForEditAsync(IEnumerable<int> existingProductIds)
        {
            var allProducts = await _productService.GetAllAsync();
            return allProducts.Where(p => p.Stock > 0 || existingProductIds.Contains(p.Id)).ToList();
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
        [HttpGet]
        [Route("Sales/CreateEdit")]
        public async Task<IActionResult> CreateEdit()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var products = await GetAvailableProductsAsync(); // Solo productos con stock

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
        [HttpGet]
        [Route("Sales/CreateEdit/{id:int}")]
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
                var existingProductIds = sale.Details.Select(d => d.ProductId);
                var products = await GetProductsForEditAsync(existingProductIds);

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
        [Route("Sales/CreateEdit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(SaleSaveViewModel model, int? id = null, IFormFile? transferReceiptFile = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEdit = id.HasValue;
                    
                    if (isEdit)
                    {
                        // EDITAR VENTA
                        var existingSale = await _saleService.GetFullSaleAsync(id.Value);
                        if (existingSale == null)
                        {
                            TempData["Error"] = "Venta no encontrada.";
                            return RedirectToAction(nameof(Index));
                        }

                        // Update sale properties
                        existingSale.CustomerId = model.CustomerId;

                        // Actualizar pago si existe
                        var existingPayment = await _paymentService.GetBySaleIdAsync(existingSale.Id);
                        if (existingPayment != null)
                        {
                            // Procesar archivo de comprobante si es transferencia
                            if (model.PaymentMethod == PaymentMethodType.Transfer && transferReceiptFile != null)
                            {
                                // Eliminar archivo anterior si existe
                                if (!string.IsNullOrEmpty(existingPayment.TransferReceiptPath))
                                {
                                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingPayment.TransferReceiptPath.TrimStart('/'));
                                    if (System.IO.File.Exists(oldPath))
                                    {
                                        System.IO.File.Delete(oldPath);
                                    }
                                }
                                existingPayment.TransferReceiptPath = await UploadReceiptFile(transferReceiptFile);
                            }

                            // Mapear campos específicos del modelo al pago
                            if (model.PaymentMethod == PaymentMethodType.Transfer)
                            {
                                existingPayment.TransferReference = model.TransferReference;
                                existingPayment.BankAccount = model.BankAccount;
                            }
                            else if (model.PaymentMethod == PaymentMethodType.Card)
                            {
                                existingPayment.CardNumber = model.CardNumber;
                                existingPayment.CardHolderName = model.CardHolderName;
                                existingPayment.CardExpiryDate = model.CardExpiryDate;
                                existingPayment.CardCVV = model.CardCVV;
                            }

                            await _paymentService.UpdateAsync(existingPayment);
                        }

                        // Eliminar detalles existentes
                        foreach (var existingDetail in existingSale.Details)
                        {
                            // Restaurar stock de productos
                            var product = await _productService.GetByIdAsync(existingDetail.ProductId);
                            if (product != null)
                            {
                                product.Stock += existingDetail.Quantity;
                                await _productService.UpdateAsync(product);
                            }
                            await _saleDetailService.DeleteAsync(existingDetail.Id);
                        }

                        // Crear nuevos detalles
                        if (model.Details != null && model.Details.Any())
                        {
                            foreach (var detailModel in model.Details)
                            {
                                var product = await _productService.GetByIdAsync(detailModel.ProductId);
                                if (product != null && product.Stock >= detailModel.Quantity)
                                {
                                    // Crear el detalle de venta
                                    var saleDetail = new SaleDetail
                                    {
                                        SaleId = existingSale.Id,
                                        ProductId = detailModel.ProductId,
                                        Quantity = detailModel.Quantity,
                                        Subtotal = product.Price * detailModel.Quantity
                                    };

                                    await _saleDetailService.CreateAsync(saleDetail);

                                    // Actualizar stock del producto
                                    product.Stock -= detailModel.Quantity;
                                    await _productService.UpdateAsync(product);
                                }
                            }
                        }

                        await _saleService.UpdateAsync(existingSale);

                        TempData["Success"] = $"Venta #{existingSale.Id:D4} actualizada exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // CREAR VENTA
                    var sale = new Sale
                    {
                        CustomerId = model.CustomerId,
                        SaleDate = DateTime.Now,
                            TotalAmount = model.AmountPaid
                        };

                        await _saleService.CreateAsync(sale);

                        // Crear detalles de venta
                        if (model.Details != null && model.Details.Any())
                        {
                            foreach (var detailModel in model.Details)
                            {
                                var product = await _productService.GetByIdAsync(detailModel.ProductId);
                                if (product != null && product.Stock >= detailModel.Quantity)
                                {
                                    // Crear el detalle de venta
                                    var saleDetail = new SaleDetail
                                    {
                                        SaleId = sale.Id,
                                        ProductId = detailModel.ProductId,
                                        Quantity = detailModel.Quantity,
                                        Subtotal = product.Price * detailModel.Quantity
                                    };

                                    await _saleDetailService.CreateAsync(saleDetail);

                                    // Actualizar stock del producto
                                    product.Stock -= detailModel.Quantity;
                                    await _productService.UpdateAsync(product);
                                }
                            }
                        }

                        // Crear pago automáticamente
                        var payment = new Payment
                        {
                            SaleId = sale.Id,
                            PaymentMethod = model.PaymentMethod,
                            Amount = model.AmountPaid,
                            PaymentDate = DateTime.Now,
                            Status = PaymentStatusType.Completed // Pago completado inmediatamente
                        };

                        // Procesar archivo de comprobante si es transferencia
                        if (model.PaymentMethod == PaymentMethodType.Transfer && transferReceiptFile != null)
                        {
                            Console.WriteLine($"Procesando archivo de transferencia: {transferReceiptFile.FileName}");
                            payment.TransferReceiptPath = await UploadReceiptFile(transferReceiptFile);
                            Console.WriteLine($"Path guardado: {payment.TransferReceiptPath}");
                        }
                        else if (model.PaymentMethod == PaymentMethodType.Transfer)
                        {
                            Console.WriteLine("Método de pago es Transfer pero no hay archivo");
                        }

                        // Mapear campos específicos del modelo al pago
                        if (model.PaymentMethod == PaymentMethodType.Transfer)
                        {
                            payment.TransferReference = model.TransferReference;
                            payment.BankAccount = model.BankAccount;
                        }
                        else if (model.PaymentMethod == PaymentMethodType.Card)
                        {
                            payment.CardNumber = model.CardNumber;
                            payment.CardHolderName = model.CardHolderName;
                            payment.CardExpiryDate = model.CardExpiryDate;
                            payment.CardCVV = model.CardCVV;
                        }

                        await _paymentService.CreateAsync(payment);

                        TempData["Success"] = $"Venta #{sale.Id:D4} creada exitosamente con pago procesado.";
                    return RedirectToAction(nameof(Index));
                }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar la venta: " + ex.Message;
            }

            return await ReloadCreateEditView(model, id.HasValue, id);
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

        private async Task<IActionResult> ReloadCreateEditView(SaleSaveViewModel model, bool isEdit, int? id = null)
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var products = await GetAvailableProductsAsync();

                ViewBag.Customers = customers;
                ViewBag.Products = products;
                ViewBag.PaymentMethods = Enum.GetValues(typeof(PaymentMethodType)).Cast<PaymentMethodType>();
                ViewBag.IsEdit = isEdit;
                ViewBag.SaleId = id;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sales/Print/5
        public async Task<IActionResult> Print(int id)
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
                ViewBag.IsPrintView = true; // Para aplicar estilos de impresión
                return View("Print", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar la impresión: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> UploadReceiptFile(IFormFile file)
        {
            Console.WriteLine($"UploadReceiptFile llamado con archivo: {file?.FileName}");
            
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("Archivo es null o está vacío");
                return null;
            }

            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "transfer-receipts");
            Console.WriteLine($"Carpeta de upload: {uploadFolder}");
            
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
                Console.WriteLine("Carpeta creada");
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);
            Console.WriteLine($"Archivo a guardar: {filePath}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var returnPath = $"/uploads/transfer-receipts/{fileName}";
            Console.WriteLine($"Path retornado: {returnPath}");
            return returnPath;
        }
    }
} 