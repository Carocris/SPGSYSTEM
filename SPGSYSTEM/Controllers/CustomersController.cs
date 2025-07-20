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
            var customers = await _customerService.GetAllAsync();
            var customerViewModels = _mapper.Map<List<CustomerViewModel>>(customers);
            return View(customerViewModels);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = _mapper.Map<CustomerViewModel>(customer);
            return View(customerViewModel);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View(new CustomerSaveViewModel());
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerSaveViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _mapper.Map<Customer>(model);
                await _customerService.CreateAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerSaveViewModel = _mapper.Map<CustomerSaveViewModel>(customer);
            ViewBag.CustomerId = id;
            ViewBag.IsEdit = true;
            return View("Create", customerSaveViewModel);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerSaveViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _mapper.Map<Customer>(model);
                customer.Id = id;
                await _customerService.UpdateAsync(customer);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerId = id;
            ViewBag.IsEdit = true;
            return View("Create", model);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = _mapper.Map<CustomerViewModel>(customer);
            return View(customerViewModel);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
} 