using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize(Roles = "Admin")] // Admin only
    public class ShiftTypesController : Controller
    {
        private readonly IApiClientService _apiClient;

        public ShiftTypesController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: ShiftTypes
        public async Task<IActionResult> Index()
        {
            var shiftTypes = await _apiClient.GetShiftTypesAsync(); // Uses existing method
            return View(shiftTypes);
        }

        // GET: ShiftTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var shiftType = await _apiClient.GetShiftTypeByIdAsync(id.Value);
            if (shiftType == null) return NotFound();
            return View(shiftType);
        }

        // GET: ShiftTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ShiftTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeName,IsOnCall,Description")] CreateShiftTypeDto shiftTypeDto)
        {
            if (ModelState.IsValid)
            {
                var createdShiftType = await _apiClient.CreateShiftTypeAsync(shiftTypeDto);
                if (createdShiftType != null)
                {
                    TempData["SuccessMessage"] = $"Shift Type '{createdShiftType.TypeName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create Shift Type via API. The name might already exist.");
                }
            }
            return View(shiftTypeDto);
        }

        // GET: ShiftTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var shiftType = await _apiClient.GetShiftTypeByIdAsync(id.Value);
            if (shiftType == null) return NotFound();

            // Map DTO to Update DTO
            var updateDto = new UpdateShiftTypeDto
            {
                TypeName = shiftType.TypeName,
                IsOnCall = shiftType.IsOnCall,
                Description = shiftType.Description
            };
            return View(updateDto);
        }

        // POST: ShiftTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TypeName,IsOnCall,Description")] UpdateShiftTypeDto shiftTypeDto)
        {
            if (ModelState.IsValid)
            {
                bool success = await _apiClient.UpdateShiftTypeAsync(id, shiftTypeDto);
                if (success)
                {
                    TempData["SuccessMessage"] = "Shift Type updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var exists = await _apiClient.GetShiftTypeByIdAsync(id);
                    if (exists == null) return NotFound();
                    ModelState.AddModelError(string.Empty, "Failed to update Shift Type via API. The name might conflict.");
                }
            }
            return View(shiftTypeDto);
        }

        // GET: ShiftTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var shiftType = await _apiClient.GetShiftTypeByIdAsync(id.Value);
            if (shiftType == null) return NotFound();
            return View(shiftType);
        }

        // POST: ShiftTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool success = await _apiClient.DeleteShiftTypeAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Shift Type deleted successfully. Existing shifts using this type will now have no type assigned."; // Inform about SET NULL
            }
            else
            {
                var exists = await _apiClient.GetShiftTypeByIdAsync(id);
                if (exists == null) TempData["ErrorMessage"] = "Shift Type was already deleted.";
                else TempData["ErrorMessage"] = "Failed to delete Shift Type. Check API logs.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
