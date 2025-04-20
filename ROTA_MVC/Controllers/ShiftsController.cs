using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize] 
    public class ShiftsController : Controller
    {
        private readonly IApiClientService _apiClient;

        public ShiftsController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        private async Task PopulateDropdownsAsync(object? selectedEmployee = null, object? selectedShiftType = null)
        {
            var employees = await _apiClient.GetActiveEmployeesBriefAsync();
            var shiftTypes = await _apiClient.GetShiftTypesAsync();

            ViewBag.EmployeeId = new SelectList(employees.Select(e => new { e.EmployeeId, FullName = $"{e.FirstName} {e.LastName}" }), "EmployeeId", "FullName", selectedEmployee);
            ViewBag.ShiftTypeId = new SelectList(shiftTypes, "ShiftTypeId", "TypeName", selectedShiftType);
        }


        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var effectiveStartDate = startDate ?? today.AddDays(-(int)today.DayOfWeek);
            var effectiveEndDate = endDate ?? effectiveStartDate.AddDays(6);
            ViewBag.StartDate = effectiveStartDate;
            ViewBag.EndDate = effectiveEndDate;
            ViewBag.EmployeeIdFilter = employeeId;
            var employeesForDropdown = await _apiClient.GetActiveEmployeesBriefAsync();            
            ViewBag.EmployeeList = new SelectList(employeesForDropdown, "EmployeeId", "FullName", employeeId);          
            var shifts = await _apiClient.GetShiftsAsync(effectiveStartDate, effectiveEndDate, employeeId, null, null);
            return View(shifts);
        }

        // GET: /Shifts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var shift = await _apiClient.GetShiftByIdAsync(id.Value);
            if (shift == null) return NotFound();
            return View(shift);
        }

        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync(); 
            var defaultShift = new CreateShiftDto
            {
                ShiftStartDateTime = DateTime.Today.AddHours(9),
                ShiftEndDateTime = DateTime.Today.AddHours(17)
            };
            return View(defaultShift);
        }

        // POST: /Shifts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EmployeeId,ShiftTypeId,ShiftStartDateTime,ShiftEndDateTime,Notes")] CreateShiftDto shiftDto)
        {
            // Basic check before calling API
            if (shiftDto.ShiftEndDateTime <= shiftDto.ShiftStartDateTime)
            {
                ModelState.AddModelError("ShiftEndDateTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                var createdShift = await _apiClient.CreateShiftAsync(shiftDto);
                if (createdShift != null)
                {
                    TempData["SuccessMessage"] = "Shift created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // API call failed (e.g., 400 Bad Request from API overlap check)
                    ModelState.AddModelError(string.Empty, "Failed to create shift via API. Possible overlap or invalid data.");
                }
            }
            // If we got this far, something failed, redisplay form with dropdowns
            await PopulateDropdownsAsync(shiftDto.EmployeeId, shiftDto.ShiftTypeId);
            return View(shiftDto);
        }


        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var shift = await _apiClient.GetShiftByIdAsync(id.Value); // Get current data
            if (shift == null) return NotFound();

            var updateDto = new UpdateShiftDto
            {
                EmployeeId = shift.EmployeeId,
                ShiftTypeId = shift.ShiftTypeId,
                ShiftStartDateTime = shift.ShiftStartDateTime,
                ShiftEndDateTime = shift.ShiftEndDateTime,
                Notes = shift.Notes
            };
            await PopulateDropdownsAsync(updateDto.EmployeeId, updateDto.ShiftTypeId); // Populate dropdowns
            return View(updateDto);
        }

        // POST: /Shifts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,ShiftTypeId,ShiftStartDateTime,ShiftEndDateTime,Notes")] UpdateShiftDto shiftDto)
        {
            // Basic check before calling API
            if (shiftDto.ShiftEndDateTime <= shiftDto.ShiftStartDateTime)
            {
                ModelState.AddModelError("ShiftEndDateTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                bool success = await _apiClient.UpdateShiftAsync(id, shiftDto);
                if (success)
                {
                    TempData["SuccessMessage"] = "Shift updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // API call failed
                    var exists = await _apiClient.GetShiftByIdAsync(id);
                    if (exists == null) return NotFound(); // Treat as NotFound if it disappeared

                    ModelState.AddModelError(string.Empty, "Failed to update shift via API. Possible overlap or invalid data.");
                }
            }
            await PopulateDropdownsAsync(shiftDto.EmployeeId, shiftDto.ShiftTypeId); // Repopulate dropdowns on failure
            return View(shiftDto);
        }


        // --- DELETE Actions ---
        // GET: /Shifts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var shift = await _apiClient.GetShiftByIdAsync(id.Value);
            if (shift == null) return NotFound();
            return View(shift); // Show confirmation view
        }

        // POST: /Shifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool success = await _apiClient.DeleteShiftAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = $"Shift deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to delete shift with ID {id} via API.";
                var shift = await _apiClient.GetShiftByIdAsync(id);
                if (shift == null) return RedirectToAction(nameof(Index)); // Already gone

                return RedirectToAction(nameof(Delete), new { id = id }); // Show confirmation again with error
            }
        }
        private async Task PopulateCalendarFilterDropdownsAsync()
        {
            if (User.IsInRole("Admin"))
            {
                // Pass the full Employee DTO list to the view for the dropdown
                var employees = await _apiClient.GetActiveEmployeesBriefAsync();
                // ViewBag.EmployeeList = new SelectList(employees.Select(e => new { e.EmployeeId, FullName = $"{e.FirstName} {e.LastName}" }), "EmployeeId", "FullName");
                ViewData["EmployeeList"] = employees; // Pass raw list for @foreach loop in view

                var teams = await _apiClient.GetTeamsAsync();
                ViewBag.TeamList = new SelectList(teams, "TeamId", "TeamName");
            }
            else
            {
                // Non-admins don't get team/employee filters in this example
                ViewData["EmployeeList"] = Enumerable.Empty<EmployeeDto>();
                ViewBag.TeamList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            await PopulateCalendarFilterDropdownsAsync(); // Call helper

            // Pass employee list for direct iteration if needed by that dropdown approach
            var employeesForFilter = User.IsInRole("Admin") ? await _apiClient.GetActiveEmployeesBriefAsync() : Enumerable.Empty<EmployeeDto>();

            return View(employeesForFilter); // Pass employee list to the view model binding
        }

    }
}
