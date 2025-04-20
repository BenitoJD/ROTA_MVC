using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;
using System.Security.Claims;

namespace ROTA_MVC.Controllers
{
    [Authorize(Roles = "Admin")] // ALL actions require Admin role
    [Route("Admin/Users")] // Use route attribute for clarity: /Admin/Users/...
    public class UsersAdminController : Controller
    {
        private readonly IApiClientService _apiClient;

        public UsersAdminController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // Helper
        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // GET: Admin/Users
        [HttpGet("")] // Maps to the base route
        public async Task<IActionResult> Index()
        {
            var users = await _apiClient.GetUsersAsync();
            return View(users);
        }

        // GET: Admin/Users/Details/5
        [HttpGet("Details/{userId}")]
        public async Task<IActionResult> Details(int userId)
        {
            var user = await _apiClient.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        // --- EDIT ROLE ACTIONS ---

        // GET: Admin/Users/EditRole/5
        [HttpGet("EditRole/{userId}")]
        public async Task<IActionResult> EditRole(int userId)
        {
            var user = await _apiClient.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            // Prepare DTO for the form
            var model = new UpdateUserRoleDto { RoleId = user.RoleId };

            // Populate dropdown
            ViewBag.RoleList = new SelectList(await _apiClient.GetRolesAsync(), "RoleId", "RoleName", user.RoleId);
            ViewBag.UserName = user.Username; // For display in view title/heading

            return View(model);
        }

        // POST: Admin/Users/EditRole/5
        [HttpPost("EditRole/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(int userId, [Bind("RoleId")] UpdateUserRoleDto roleDto)
        {
            if (ModelState.IsValid)
            {
                // Optional safety check: prevent admin changing own role (API should also check this)
                // if (userId == GetCurrentUserId()) {
                //     ModelState.AddModelError("", "Cannot change your own role.");
                //     // Repopulate and return view
                // }

                bool success = await _apiClient.UpdateUserRoleAsync(userId, roleDto);
                if (success)
                {
                    TempData["SuccessMessage"] = "User role updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var user = await _apiClient.GetUserByIdAsync(userId); // Check if user still exists
                    if (user == null) return NotFound();
                    ModelState.AddModelError("", "Failed to update user role via API. Ensure Role ID is valid.");
                }
            }
            // Repopulate dropdown on failure
            var failedUser = await _apiClient.GetUserByIdAsync(userId); // Get user details again for context
            ViewBag.RoleList = new SelectList(await _apiClient.GetRolesAsync(), "RoleId", "RoleName", roleDto.RoleId);
            ViewBag.UserName = failedUser?.Username ?? "User";
            return View(roleDto);
        }


        // --- ACTIVATE/DEACTIVATE ACTIONS (using EditStatus view) ---

        // GET: Admin/Users/EditStatus/5
        [HttpGet("EditStatus/{userId}")]
        public async Task<IActionResult> EditStatus(int userId)
        {
            var user = await _apiClient.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            // Prepare DTO for the form
            var model = new UpdateUserStatusDto { IsActive = user.IsActive };
            ViewBag.UserName = user.Username;

            return View(model);
        }

        // POST: Admin/Users/EditStatus/5
        [HttpPost("EditStatus/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int userId, [Bind("IsActive")] UpdateUserStatusDto statusDto)
        {
            // Mandatory safety check: prevent admin deactivating themselves!
            if (userId == GetCurrentUserId() && !statusDto.IsActive)
            {
                ModelState.AddModelError("IsActive", "Administrators cannot deactivate their own account.");
            }

            if (ModelState.IsValid)
            {
                bool success = await _apiClient.UpdateUserStatusAsync(userId, statusDto);
                if (success)
                {
                    TempData["SuccessMessage"] = $"User account {(statusDto.IsActive ? "activated" : "deactivated")} successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var user = await _apiClient.GetUserByIdAsync(userId); // Check if user still exists
                    if (user == null) return NotFound();
                    ModelState.AddModelError("", "Failed to update user status via API.");
                }
            }
            // Repopulate viewbag needed by view
            var failedUser = await _apiClient.GetUserByIdAsync(userId);
            ViewBag.UserName = failedUser?.Username ?? "User";
            return View(statusDto);
        }
        // GET: Admin/Users/Register
        [HttpGet("Register")]
        public async Task<IActionResult> Register()
        {
            await PopulateRegisterDropdownsAsync();
            return View(new RegisterUserDto()); // Pass empty DTO to the view
        }

        // POST: Admin/Users/Register
        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("EmployeeId,Username,Password,ConfirmPassword,RoleId")] RegisterUserDto registerDto)
        {
            // Basic check - does password match confirmation? (DTO annotation should handle this too)
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            }

            if (ModelState.IsValid)
            {
                var createdUserMarker = await _apiClient.RegisterUserAsync(registerDto); // Call the service
                if (createdUserMarker != null) // Service returns non-null on success in this version
                {
                    TempData["SuccessMessage"] = $"User '{registerDto.Username}' registered successfully.";
                    return RedirectToAction(nameof(Index)); // Redirect to user list
                }
                else
                {
                    // API call failed (e.g., 400 Bad Request - username exists, employee already has user, invalid ID)
                    // TODO: Get specific error from API if possible and add to ModelState
                    ModelState.AddModelError(string.Empty, "Failed to register user via API. Username might exist, Employee might already have an account, or an ID might be invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            await PopulateRegisterDropdownsAsync(registerDto.EmployeeId, registerDto.RoleId); // Repopulate dropdowns
            return View(registerDto);
        }
        private async Task PopulateRegisterDropdownsAsync(object? selectedEmployee = null, object? selectedRole = null)
        {
            // Get employees who DON'T already have a user account
            // This requires more complex logic - either filter API-side or fetch all users/employees client-side
            // Simple approach for now: Fetch all active employees. Validation will happen API-side on submit.
            ViewBag.EmployeeList = new SelectList(
                (await _apiClient.GetActiveEmployeesBriefAsync())
                    .Select(e => new { e.EmployeeId, FullName = $"{e.FirstName} {e.LastName}" }),
                "EmployeeId",
                "FullName",
                selectedEmployee);

            ViewBag.RoleList = new SelectList(await _apiClient.GetRolesAsync(), "RoleId", "RoleName", selectedRole);
        }
    }
}