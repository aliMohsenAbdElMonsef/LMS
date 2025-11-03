using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
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
            var category = await _services.CategoryService.GetCategoryById(id);
            if (category == null)
                return NotFound();

            return View("Details", category);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id) 
        {
           var category = await _services.CategoryService.GetEditModel(id);
            if (category == null) return NotFound();
            return View("Edit", category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                    return RedirectToAction("Details", new { id = updatedCategory.Id });
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = _services.CategoryService.GetCreateModel();
            
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                return View("Create");
            }

            try
            {
                var CreatedCategory = await _services.CategoryService.CreateCategory(model);
                if (CreatedCategory != null && !string.IsNullOrEmpty(CreatedCategory.Id))
                {
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction("Details", new { id = CreatedCategory.Id });
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
    }
}
