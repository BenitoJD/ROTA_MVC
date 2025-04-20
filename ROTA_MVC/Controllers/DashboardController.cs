using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ROTA_MVC.Models;
using ROTA_MVC.Services;

namespace ROTA_MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IApiClientService _apiClient;

        public DashboardController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var onCallStartDate = today;
            var onCallEndDate = today.AddDays(6); // Next 7 days for on-call
            var summaryStartDate = today.AddDays(-29); // Past 30 days for summaries
            var summaryEndDate = today;

           
            var pendingCountTask = Task.FromResult(Enumerable.Empty<PendingCountDto>()); // Default empty
            if (User.IsInRole("Admin"))
            {
                pendingCountTask = _apiClient.GetPendingLeaveCountAsync(null); // Overall count
            }

            var onCallTask = _apiClient.GetUpcomingOnCallAsync(onCallStartDate, onCallEndDate, null);

            var leaveSummaryParams = new LeaveSummaryRequestParams
            {
                StartDate = summaryStartDate,
                EndDate = summaryEndDate,
                GroupBy = LeaveSummaryGrouping.LeaveType // Group by type for this dashboard view
            };
            var leaveSummaryTask = _apiClient.GetLeaveSummaryAsync(leaveSummaryParams);

            var shiftDistributionTask = _apiClient.GetShiftTypeDistributionAsync(summaryStartDate, summaryEndDate, null); // Overall distribution

            // Await all tasks
            await Task.WhenAll(pendingCountTask, onCallTask, leaveSummaryTask, shiftDistributionTask);

            // --- Create the ViewModel ---
            var viewModel = new DashboardViewModel
            {
                // Assign results from completed tasks
                PendingLeaveCounts = await pendingCountTask,
                UpcomingOnCall = await onCallTask,
                OnCallStartDate = onCallStartDate,
                OnCallEndDate = onCallEndDate,
                LeaveSummaryByType = await leaveSummaryTask,
                ShiftTypeDistribution = await shiftDistributionTask,
                SummaryPeriodStartDate = summaryStartDate,
                SummaryPeriodEndDate = summaryEndDate
            };

            return View(viewModel);
        }
    }
}
