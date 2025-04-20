using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize] // All actions require user to be logged in
    public class ManageController : Controller
    {
        private readonly IApiClientService _apiClient;

        public ManageController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Manage/ or /Manage/Index (My Profile)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Call API client to get current user's profile details
            var profile = await _apiClient.GetMyProfileAsync();

            if (profile == null)
            {
                // Handle case where profile couldn't be fetched (API error, user deleted?)
                TempData["ErrorMessage"] = "Could not retrieve your profile information.";
                // Maybe log out user or redirect home?
                return RedirectToAction("Index", "Home");
            }

            return View(profile); // Pass UserDetailDto to the view
        }


        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(); // Show the empty change password form
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return view with validation errors
            }

            var (success, errorMessage) = await _apiClient.ChangePasswordAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                // Optionally sign the user out so they have to log in with the new password
                // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // return RedirectToAction("Login", "Auth");
                return RedirectToAction(nameof(Index)); // Redirect to profile page
            }
            else
            {
                // Add the error message returned from the API client service
                ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to change password due to an unknown error.");
                return View(model); // Show form again with error message
            }
        }
    }
}
