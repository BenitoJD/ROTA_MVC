using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;


namespace ROTA_MVC.Controllers
{
    [Authorize(Roles = "Admin")] // ALL actions require Admin role
    public class TeamsController : Controller
    {
        private readonly IApiClientService _apiClient;

        public TeamsController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Teams/ or /Teams/Index
        public async Task<IActionResult> Index()
        {
            var teams = await _apiClient.GetTeamsAsync();
            return View(teams); // Pass List<TeamDto> to the view
        }

        // GET: /Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            // We need a GetTeamById method in the ApiClientService
            var team = await _apiClient.GetTeamByIdAsync(id.Value); // Add this method call
            if (team == null) return NotFound($"Team with ID {id.Value} not found via API.");
            return View(team);
        }

        // GET: /Teams/Create
        public IActionResult Create()
        {
            // Simple view with form fields
            return View();
        }

        // POST: /Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamName,Description")] CreateTeamDto teamDto)
        {
            if (ModelState.IsValid)
            {
                // Need a CreateTeamAsync method in the ApiClientService
                var createdTeam = await _apiClient.CreateTeamAsync(teamDto); // Add this method call
                if (createdTeam != null)
                {
                    TempData["SuccessMessage"] = $"Team '{createdTeam.TeamName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // API call failed (e.g., 400 Bad Request - name exists?)
                    // TODO: Get specific error from API if possible
                    ModelState.AddModelError(string.Empty, "Failed to create team via API. The team name might already exist.");
                }
            }
            return View(teamDto); // Redisplay form with errors
        }

        // GET: /Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var team = await _apiClient.GetTeamByIdAsync(id.Value); // Use existing GetTeamByIdAsync call
            if (team == null) return NotFound();

            // Map TeamDto to UpdateTeamDto for the form
            var updateDto = new UpdateTeamDto
            {
                TeamName = team.TeamName,
                Description = team.Description
            };
            return View(updateDto);
        }

        // POST: /Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamName,Description")] UpdateTeamDto teamDto)
        {
            if (ModelState.IsValid)
            {
                // Need an UpdateTeamAsync method in the ApiClientService
                bool success = await _apiClient.UpdateTeamAsync(id, teamDto); // Add this method call
                if (success)
                {
                    TempData["SuccessMessage"] = "Team updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // API call failed (404, 400 name exists, 500?)
                    var exists = await _apiClient.GetTeamByIdAsync(id);
                    if (exists == null) return NotFound();
                    ModelState.AddModelError(string.Empty, "Failed to update team via API. The name might conflict with another team.");
                }
            }
            return View(teamDto); // Redisplay form with errors
        }


        // GET: /Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var team = await _apiClient.GetTeamByIdAsync(id.Value); // Use existing GetTeamByIdAsync call
            if (team == null) return NotFound();
            return View(team); // Show confirmation view
        }

        // POST: /Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Need a DeleteTeamAsync method in the ApiClientService
            bool success = await _apiClient.DeleteTeamAsync(id); // Add this method call
            if (success)
            {
                TempData["SuccessMessage"] = "Team deleted successfully. Associated employees are now unassigned.";
            }
            else
            {
                // API call failed (404, 400 if prevented by API rule, 500?)
                var exists = await _apiClient.GetTeamByIdAsync(id);
                if (exists == null) TempData["ErrorMessage"] = "Team was already deleted.";
                else TempData["ErrorMessage"] = "Failed to delete team via API. Check API logs.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}