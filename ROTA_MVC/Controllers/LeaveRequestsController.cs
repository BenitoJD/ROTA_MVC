using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ROTA_MVC.Models;
using ROTA_MVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; 
using System.Threading.Tasks;

namespace ROTA_MVC.Controllers
{
    [Authorize] 
    public class LeaveRequestsController : Controller
    {
        private readonly IApiClientService _apiClient;

        public LeaveRequestsController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        #region Helper Methods

        // Helper to check if the current user has the Admin role
        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        // Helper to get current user's Employee ID directly from Claims
        // Assumes the 'employeeId' claim was added during API login and copied to MVC cookie
        private int? GetCurrentUserEmployeeIdFromClaims()
        {
            var employeeIdClaim = User.FindFirstValue("employeeId"); // Use the claim name set in API AuthController

            if (employeeIdClaim != null && int.TryParse(employeeIdClaim, out int employeeId))
            {
                return employeeId;
            }
            // Log this warning if claim is missing for a logged-in user (potential config issue)
            Console.WriteLine("Warning: Could not find or parse 'employeeId' claim for current user.");
            return null;
        }

        // Helper to populate filter dropdowns for the Index view
        private async Task PopulateFilterDropdownsAsync(object? selectedEmployee = null, object? selectedTeam = null, object? selectedLeaveType = null, object? selectedStatus = null)
        {
            // Only populate Employee/Team list for Admins
            if (User.IsInRole("Admin"))
            {
                var employees = await _apiClient.GetActiveEmployeesBriefAsync();
                ViewBag.EmployeeList = new SelectList(employees.Select(e => new { e.EmployeeId, FullName = $"{e.FirstName} {e.LastName}" }), "EmployeeId", "FullName", selectedEmployee);
                ViewBag.TeamList = new SelectList(await _apiClient.GetTeamsAsync(), "TeamId", "TeamName", selectedTeam);
            }
            else
            {
                ViewBag.EmployeeList = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.TeamList = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            ViewBag.LeaveTypeList = new SelectList(await _apiClient.GetLeaveTypesAsync(), "LeaveTypeId", "LeaveTypeName", selectedLeaveType);
            // Create SelectList for Status enum
            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(LeaveStatus)).Cast<LeaveStatus>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }), "Value", "Text", selectedStatus);
        }

        // Helper to populate dropdowns for Create/Edit forms
        private async Task PopulateLeaveFormDropdownsAsync(object? selectedEmployee = null, object? selectedLeaveType = null)
        {
            // Only Admins need the full employee list in the Create form
            if (User.IsInRole("Admin"))
            {
                var employees = await _apiClient.GetActiveEmployeesBriefAsync();
                ViewBag.EmployeeId = new SelectList(employees.Select(e => new { e.EmployeeId, FullName = $"{e.FirstName} {e.LastName}" }), "EmployeeId", "FullName", selectedEmployee);
            }
            ViewBag.LeaveTypeId = new SelectList(await _apiClient.GetLeaveTypesAsync(), "LeaveTypeId", "LeaveTypeName", selectedLeaveType);
        }

        #endregion

        #region Controller Actions

        // GET: /LeaveRequests/ or /LeaveRequests/Index
        public async Task<IActionResult> Index(
            DateTime? startDate, DateTime? endDate,
            int? employeeId, int? teamId, int? leaveTypeId, LeaveStatus? status)
        {
            var today = DateTime.UtcNow.Date;
            var effectiveStartDate = startDate ?? new DateTime(today.Year, today.Month, 1);
            var effectiveEndDate = endDate ?? effectiveStartDate.AddMonths(1).AddDays(-1);

            bool isAdmin = IsCurrentUserAdmin();
            int? currentUserEmployeeId = null;

            // Assign current user's Employee ID to ViewBag for use in the View's Cancel logic
            if (!isAdmin)
            {
                currentUserEmployeeId = GetCurrentUserEmployeeIdFromClaims();
            }
            ViewBag.CurrentUserEmployeeId = currentUserEmployeeId;


            // If filtering by EmployeeId, only Admins should be able to select someone else.
            if (!isAdmin)
            {
                employeeId = null; // Force non-admins API call to filter implicitly by their identity if service supports it
                teamId = null;     // Non-admins typically shouldn't filter by team either
            }

            // Populate filters for the view
            ViewBag.StartDate = effectiveStartDate;
            ViewBag.EndDate = effectiveEndDate;
            ViewBag.EmployeeIdFilter = employeeId;
            ViewBag.TeamIdFilter = teamId;
            ViewBag.LeaveTypeIdFilter = leaveTypeId;
            ViewBag.StatusFilter = status;
            await PopulateFilterDropdownsAsync(employeeId, teamId, leaveTypeId, status != null ? (int?)status : null);

            var leaveRequests = await _apiClient.GetLeaveRequestsAsync(
                effectiveStartDate, effectiveEndDate,
                employeeId, teamId, leaveTypeId, status); // API service handles actual filtering based on role/params

            return View(leaveRequests);
        }

        // GET: /LeaveRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Pass current user's Employee ID for potential display logic in the view
            bool isAdmin = IsCurrentUserAdmin();
            int? currentUserEmployeeId = null;
            if (!isAdmin)
            {
                currentUserEmployeeId = GetCurrentUserEmployeeIdFromClaims();
            }
            ViewBag.CurrentUserEmployeeId = currentUserEmployeeId;

            var leaveRequestDto = await _apiClient.GetLeaveRequestByIdAsync(id.Value);

            if (leaveRequestDto == null)
            {
                // Could be Not Found or Forbidden by API/Service
                TempData["ErrorMessage"] = $"Leave Request with ID {id.Value} not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            // Optional extra check in controller (belt-and-suspenders)
            if (!isAdmin && (currentUserEmployeeId == null || leaveRequestDto.EmployeeId != currentUserEmployeeId.Value))
            {
                return Forbid("Access denied to view this leave request.");
            }

            return View(leaveRequestDto);
        }


        // GET: /LeaveRequests/Create
        public async Task<IActionResult> Create()
        {
            await PopulateLeaveFormDropdownsAsync();
            var model = new CreateLeaveRequestDto();

            if (!User.IsInRole("Admin"))
            {
                var currentUserEmployeeId = GetCurrentUserEmployeeIdFromClaims();
                if (currentUserEmployeeId.HasValue)
                {
                    model.EmployeeId = currentUserEmployeeId.Value;
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot create leave request: Your user account is not linked to an employee record or claim is missing.";
                    return RedirectToAction(nameof(Index));
                }
            }

            model.LeaveStartDateTime = DateTime.Today.AddDays(1).AddHours(9);
            model.LeaveEndDateTime = DateTime.Today.AddDays(1).AddHours(17);

            return View(model);
        }

        // POST: /LeaveRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,LeaveTypeId,LeaveStartDateTime,LeaveEndDateTime,Reason")] CreateLeaveRequestDto leaveRequestDto)
        {
            bool isAdmin = IsCurrentUserAdmin();
            // Re-validate EmployeeId for non-admins server-side
            if (!isAdmin)
            {
                var currentUserEmployeeId = GetCurrentUserEmployeeIdFromClaims();
                if (currentUserEmployeeId == null || leaveRequestDto.EmployeeId != currentUserEmployeeId.Value)
                {
                    ModelState.AddModelError("EmployeeId", "You can only submit leave requests for yourself.");
                }
            }
            if (leaveRequestDto.LeaveEndDateTime <= leaveRequestDto.LeaveStartDateTime)
            {
                ModelState.AddModelError("LeaveEndDateTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                var createdRequest = await _apiClient.CreateLeaveRequestAsync(leaveRequestDto);
                if (createdRequest != null)
                {
                    TempData["SuccessMessage"] = "Leave request submitted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Attempt to get error details if API returns ProblemDetails (requires ApiClientService update)
                    // var errorDetails = await _apiClient.GetLastErrorDetailsAsync();
                    ModelState.AddModelError(string.Empty, "Failed to submit leave request via API. Please check details (e.g., overlapping leave/shifts or permissions).");
                }
            }
            await PopulateLeaveFormDropdownsAsync(leaveRequestDto.EmployeeId, leaveRequestDto.LeaveTypeId);
            return View(leaveRequestDto);
        }


        // --- APPROVE ACTIONS ---
        // GET: /LeaveRequests/Approve/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();
            var leaveRequestDto = await _apiClient.GetLeaveRequestByIdAsync(id.Value);
            if (leaveRequestDto == null || leaveRequestDto.Status != LeaveStatus.Pending)
            {
                TempData["ErrorMessage"] = "Leave request not found or cannot be approved.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LeaveRequest = leaveRequestDto;
            return View("UpdateStatus", new UpdateLeaveStatusDto { NewStatus = LeaveStatus.Approved });
        }

        // --- REJECT ACTIONS ---
        // GET: /LeaveRequests/Reject/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null) return NotFound();
            var leaveRequestDto = await _apiClient.GetLeaveRequestByIdAsync(id.Value);
            if (leaveRequestDto == null || leaveRequestDto.Status != LeaveStatus.Pending)
            {
                TempData["ErrorMessage"] = "Leave request not found or cannot be rejected.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LeaveRequest = leaveRequestDto;
            return View("UpdateStatus", new UpdateLeaveStatusDto { NewStatus = LeaveStatus.Rejected });
        }

        // --- SHARED POST for Approve/Reject ---
        // POST: /LeaveRequests/UpdateStatus/5
        [HttpPost]
        [ActionName("UpdateStatus")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusConfirmed(int id, [Bind("NewStatus,ApproverNotes")] UpdateLeaveStatusDto statusUpdateDto)
        {
            if (statusUpdateDto.NewStatus != LeaveStatus.Approved && statusUpdateDto.NewStatus != LeaveStatus.Rejected)
            {
                ModelState.AddModelError("NewStatus", "Invalid status update attempted.");
            }

            if (ModelState.IsValid)
            {
                var updatedDto = await _apiClient.UpdateLeaveRequestStatusAsync(id, statusUpdateDto);
                if (updatedDto != null)
                {
                    TempData["SuccessMessage"] = $"Leave request status updated to {updatedDto.StatusString}.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update leave request status via API. Request might no longer be pending or another error occurred.");
                    ViewBag.LeaveRequest = await _apiClient.GetLeaveRequestByIdAsync(id);
                    if (ViewBag.LeaveRequest == null)
                    { // If request disappeared after check
                        TempData["ErrorMessage"] = "Leave request not found.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // If ModelState invalid or API call failed, redisplay the view
            ViewBag.LeaveRequest = await _apiClient.GetLeaveRequestByIdAsync(id);
            if (ViewBag.LeaveRequest == null)
            { // If request disappeared
                TempData["ErrorMessage"] = "Leave request not found.";
                return RedirectToAction(nameof(Index));
            }
            return View("UpdateStatus", statusUpdateDto);
        }


        // --- CANCEL ACTIONS ---
        // GET: /LeaveRequests/Cancel/5
        [HttpGet]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();
            var leaveRequestDto = await _apiClient.GetLeaveRequestByIdAsync(id.Value);
            if (leaveRequestDto == null)
            {
                TempData["ErrorMessage"] = "Leave request not found.";
                return RedirectToAction(nameof(Index));
            }

            // Authorization Check
            bool isAdmin = IsCurrentUserAdmin();
            var currentUserEmployeeId = GetCurrentUserEmployeeIdFromClaims();
            bool canUserCancel = (leaveRequestDto.Status == LeaveStatus.Pending || leaveRequestDto.Status == LeaveStatus.Approved) &&
                                 (isAdmin || (currentUserEmployeeId.HasValue && currentUserEmployeeId.Value == leaveRequestDto.EmployeeId));

            if (!canUserCancel)
            {
                TempData["ErrorMessage"] = "You do not have permission to cancel this request or it cannot be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            return View(leaveRequestDto); // Show Views/LeaveRequests/Cancel.cshtml
        }

        // POST: /LeaveRequests/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            // Authorization check happens implicitly via API call based on token
            bool success = await _apiClient.CancelLeaveRequestAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Leave request cancelled successfully.";
            }
            else
            {
                var exists = await _apiClient.GetLeaveRequestByIdAsync(id);
                if (exists == null) TempData["ErrorMessage"] = "Leave request not found.";
                else TempData["ErrorMessage"] = "Failed to cancel leave request. It might already be processed, you may lack permission, or another error occurred.";
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}