using Application.Interfaces.Services;
using Application.ViewModels.Customer;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var viewModels = _mapper.Map<List<CustomerViewModel>>(customers);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los clientes: " + ex.Message;
                return View(new List<CustomerViewModel>());
            }
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Cliente no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CustomerViewModel>(customer);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el cliente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customers/CreateEdit
        public IActionResult CreateEdit()
        {
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Nuevo Cliente";
            return View(new CustomerSaveViewModel());
        }

        // GET: Customers/CreateEdit/5
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Cliente no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CustomerSaveViewModel>(customer);
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Cliente";
                ViewBag.CustomerId = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el cliente: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Cliente";
                return View("CreateEdit", viewModel);
            }

            try
            {
                var customer = _mapper.Map<Customer>(viewModel);
                await _customerService.CreateAsync(customer);
                
                TempData["Success"] = $"Cliente '{customer.Name}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el cliente: " + ex.Message;
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Cliente";
                return View("CreateEdit", viewModel);
            }
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Cliente";
                ViewBag.CustomerId = id;
                return View("CreateEdit", viewModel);
            }

            try
            {
                var existingCustomer = await _customerService.GetByIdAsync(id);
                if (existingCustomer == null)
                {
                    TempData["Error"] = "Cliente no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Mapear los cambios
                _mapper.Map(viewModel, existingCustomer);
                await _customerService.UpdateAsync(existingCustomer);
                
                TempData["Success"] = $"Cliente '{existingCustomer.Name}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el cliente: " + ex.Message;
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Cliente";
                ViewBag.CustomerId = id;
                return View("CreateEdit", viewModel);
            }
        }

        // POST: Customers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Cliente no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si el cliente tiene ventas asociadas
                if (customer.Sales?.Any() == true)
                {
                    TempData["Error"] = $"No se puede eliminar el cliente '{customer.Name}' porque tiene ventas asociadas.";
                    return RedirectToAction(nameof(Index));
                }

                await _customerService.DeleteAsync(id);
                TempData["Success"] = $"Cliente '{customer.Name}' eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el cliente: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint para búsqueda rápida (opcional)
        [HttpGet]
        public async Task<JsonResult> Search(string term)
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var filteredCustomers = customers
                    .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                               (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .Take(10)
                    .Select(c => new { id = c.Id, name = c.Name, email = c.Email, phone = c.Phone })
                    .ToList();

                return Json(filteredCustomers);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }
} 