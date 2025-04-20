using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IApiClientService _apiClient;

        public EmployeesController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _apiClient.GetEmployeesAsync();

            return View(employees); 
        }

        // GET: /Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); 
            }

            var employee = await _apiClient.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound(); 
            }

            return View(employee); 
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateTeamsDropdownAsync(); 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,TeamId")] CreateEmployeeDto employeeDto)
        {
            if (ModelState.IsValid)
            {
                var createdEmployee = await _apiClient.CreateEmployeeAsync(employeeDto);
                if (createdEmployee != null)
                {
                    TempData["SuccessMessage"] = $"Employee '{createdEmployee.FirstName} {createdEmployee.LastName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create employee via API. Check API logs or input.");
                }
            }
            // Repopulate dropdown if validation fails
            await PopulateTeamsDropdownAsync(employeeDto.TeamId);
            return View(employeeDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _apiClient.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            var updateDto = new UpdateEmployeeDto { /* ... map properties ... */ };

            await PopulateTeamsDropdownAsync(updateDto.TeamId); // Populate ViewBag
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,Email,PhoneNumber,TeamId,IsActive")] UpdateEmployeeDto employeeDto)
        {
            if (ModelState.IsValid)
            {
                bool success = await _apiClient.UpdateEmployeeAsync(id, employeeDto);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Employee updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var exists = await _apiClient.GetEmployeeByIdAsync(id);
                    if (exists == null) return NotFound();
                    ModelState.AddModelError(string.Empty, "Failed to update employee via API. Check API logs or input.");
                }
            }
            // Repopulate dropdown if validation fails
            await PopulateTeamsDropdownAsync(employeeDto.TeamId);
            return View(employeeDto);
        }


        // GET: /Employees/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _apiClient.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();
            return View(employee); 
        }

        // POST: /Employees/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool success = await _apiClient.DeleteEmployeeAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = $"Employee deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to delete employee with ID {id} via API.";
                var employee = await _apiClient.GetEmployeeByIdAsync(id);
                if (employee == null) return RedirectToAction(nameof(Index)); // Already gone

                return RedirectToAction(nameof(Delete), new { id = id }); 
            }
        }
        private async Task PopulateTeamsDropdownAsync(object? selectedTeam = null)
        {
            var teams = await _apiClient.GetTeamsAsync(); // Call the new service method
            ViewBag.TeamList = new SelectList(teams, "TeamId", "TeamName", selectedTeam);
        }
    }
}
