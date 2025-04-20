using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize(Roles = "Admin")] // Admin only
    public class LeaveTypesController : Controller
    {
        private readonly IApiClientService _apiClient;

        public LeaveTypesController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: LeaveTypes
        public async Task<IActionResult> Index()
        {
            var leaveTypes = await _apiClient.GetLeaveTypesAsync(); // Uses existing method
            return View(leaveTypes);
        }

        // GET: LeaveTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var leaveType = await _apiClient.GetLeaveTypeByIdAsync(id.Value);
            if (leaveType == null) return NotFound();
            return View(leaveType);
        }

        // GET: LeaveTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeaveTypeName,RequiresApproval,Description")] CreateLeaveTypeDto leaveTypeDto)
        {
            if (ModelState.IsValid)
            {
                var createdLeaveType = await _apiClient.CreateLeaveTypeAsync(leaveTypeDto);
                if (createdLeaveType != null)
                {
                    TempData["SuccessMessage"] = $"Leave Type '{createdLeaveType.LeaveTypeName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create Leave Type via API. The name might already exist.");
                }
            }
            return View(leaveTypeDto);
        }

        // GET: LeaveTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var leaveType = await _apiClient.GetLeaveTypeByIdAsync(id.Value);
            if (leaveType == null) return NotFound();

            // Map DTO to Update DTO
            var updateDto = new UpdateLeaveTypeDto
            {
                LeaveTypeName = leaveType.LeaveTypeName,
                RequiresApproval = leaveType.RequiresApproval,
                Description = leaveType.Description
            };
            return View(updateDto);
        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeaveTypeName,RequiresApproval,Description")] UpdateLeaveTypeDto leaveTypeDto)
        {
            if (ModelState.IsValid)
            {
                bool success = await _apiClient.UpdateLeaveTypeAsync(id, leaveTypeDto);
                if (success)
                {
                    TempData["SuccessMessage"] = "Leave Type updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var exists = await _apiClient.GetLeaveTypeByIdAsync(id);
                    if (exists == null) return NotFound();
                    ModelState.AddModelError(string.Empty, "Failed to update Leave Type via API. The name might conflict.");
                }
            }
            return View(leaveTypeDto);
        }

        // GET: LeaveTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var leaveType = await _apiClient.GetLeaveTypeByIdAsync(id.Value);
            if (leaveType == null) return NotFound();
            return View(leaveType);
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool success = await _apiClient.DeleteLeaveTypeAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Leave Type deleted successfully.";
            }
            else
            {
                var exists = await _apiClient.GetLeaveTypeByIdAsync(id);
                if (exists == null) TempData["ErrorMessage"] = "Leave Type was already deleted.";
                else TempData["ErrorMessage"] = "Failed to delete Leave Type. It might be in use by existing Leave Requests."; // Specific error hint
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
