using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfServices _services;

        public CategoryController(IUnitOfServices services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var result = await _services.CategoryService.GetCategoryById(id);
                if (result == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("CategoryManagement", "Dashboard");
                }

                return View("Details", result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading category: {ex.Message}";
                return RedirectToAction("CategoryManagement", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id) 
        {
            try
            {
                var category = await _services.CategoryService.GetEditModel(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("CategoryManagement", "Dashboard");
                }
                return View("Edit", category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading category for edit: {ex.Message}";
                return RedirectToAction("CategoryManagement", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ReadCategoryResult model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Property: {key}, Error: {error.ErrorMessage}");
                    }
                }

                return View("Edit", model);
            }

            try
            {
                var updatedCategory = await _services.CategoryService.EditCategory(model);
                if (updatedCategory != null && !string.IsNullOrEmpty(updatedCategory.Id))
                {
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction("CategoryManagement", "Dashboard");
                }

                ModelState.AddModelError("", "Failed to update category. Please try again.");
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while updating category: {ex.Message}");
                return View("Edit", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = _services.CategoryService.GetCreateModel();
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReadCategoryResult model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Property: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View("Create", model);
            }

            try
            {
                var createdCategory = await _services.CategoryService.CreateCategory(model);
                if (createdCategory != null && !string.IsNullOrEmpty(createdCategory.Id))
                {
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction("CategoryManagement", "Dashboard");
                }

                ModelState.AddModelError("", "Failed to create category. Please try again.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while creating category: {ex.Message}");
                return View("Create", model);
            }
        }
    }
}
