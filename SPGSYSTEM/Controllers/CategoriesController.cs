using Application.Interfaces.Services;
using Application.ViewModels.Category;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllWithProductsAsync();
                var viewModels = _mapper.Map<List<CategoryViewModel>>(categories);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las categorías: " + ex.Message;
                return View(new List<CategoryViewModel>());
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _categoryService.GetWithProductsAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Categoría no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CategoryViewModel>(category);
                
                // Cargar los productos de la categoría para mostrarlos en la vista
                if (category.Products != null)
                {
                    ViewBag.CategoryProducts = category.Products.Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description,
                        Stock = p.Stock,
                        PurchasePrice = p.PurchasePrice,
                        SalePrice = p.SalePrice,
                        SupplierName = p.Supplier?.Name ?? "Sin proveedor"
                    }).ToList();
                }
                else
                {
                    ViewBag.CategoryProducts = new List<object>();
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la categoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Nueva Categoría";
            return View("CreateEdit", new CategorySaveViewModel());
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Categoría no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CategorySaveViewModel>(category);
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Categoría";
                ViewBag.CategoryId = id;
                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la categoría: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategorySaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nueva Categoría";
                return View("CreateEdit", viewModel);
            }

            try
            {
                // Verificar si ya existe una categoría con ese nombre
                if (await _categoryService.ExistsAsync(viewModel.Name))
                {
                    ModelState.AddModelError("Name", "Ya existe una categoría con este nombre.");
                    ViewBag.IsEdit = false;
                    ViewBag.PageTitle = "Nueva Categoría";
                    return View("CreateEdit", viewModel);
                }

                var category = _mapper.Map<Category>(viewModel);
                await _categoryService.CreateAsync(category);
                
                TempData["Success"] = $"Categoría '{category.Name}' creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la categoría: " + ex.Message;
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nueva Categoría";
                return View("CreateEdit", viewModel);
            }
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategorySaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Categoría";
                ViewBag.CategoryId = id;
                return View("CreateEdit", viewModel);
            }

            try
            {
                var existingCategory = await _categoryService.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    TempData["Error"] = "Categoría no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si ya existe otra categoría con ese nombre
                if (await _categoryService.ExistsAsync(viewModel.Name, id))
                {
                    ModelState.AddModelError("Name", "Ya existe otra categoría con este nombre.");
                    ViewBag.IsEdit = true;
                    ViewBag.PageTitle = "Editar Categoría";
                    ViewBag.CategoryId = id;
                    return View("CreateEdit", viewModel);
                }

                // Mapear los cambios
                _mapper.Map(viewModel, existingCategory);
                await _categoryService.UpdateAsync(existingCategory);
                
                TempData["Success"] = $"Categoría '{existingCategory.Name}' actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la categoría: " + ex.Message;
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Categoría";
                ViewBag.CategoryId = id;
                return View("CreateEdit", viewModel);
            }
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _categoryService.GetWithProductsAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Categoría no encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si la categoría tiene productos asociados
                if (category.Products?.Any() == true)
                {
                    TempData["Error"] = $"No se puede eliminar la categoría '{category.Name}' porque tiene productos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                await _categoryService.DeleteAsync(id);
                TempData["Success"] = $"Categoría '{category.Name}' eliminada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la categoría: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Get active categories (for dropdowns)
        [HttpGet]
        public async Task<JsonResult> GetActive()
        {
            try
            {
                var categories = await _categoryService.GetActiveAsync();
                var result = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                return Json(result);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }
} 