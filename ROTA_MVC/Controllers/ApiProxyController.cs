using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add Logging
using ROTA_MVC.Models;
using ROTA_MVC.Services;


namespace ROTA_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiProxyController : ControllerBase
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<ApiProxyController> _logger; // Inject Logger

        public ApiProxyController(IApiClientService apiClient, ILogger<ApiProxyController> logger) // Inject Logger
        {
            _apiClient = apiClient;
            _logger = logger; // Assign logger
        }

        [HttpGet("shifts")]
        public async Task<IActionResult> GetShiftsForCalendar(
            [FromQuery] string start, [FromQuery] string end,
            [FromQuery] int? employeeId = null, [FromQuery] int? teamId = null, [FromQuery] bool? isOnCall = null)
        {
            _logger.LogInformation("Proxy GetShiftsForCalendar called. StartParam: '{StartParam}', EndParam: '{EndParam}'", start, end); // Log params

            // Try parsing dates
            if (!DateTime.TryParse(start, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime startDate) ||
                !DateTime.TryParse(end, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime endDate))
            {
                _logger.LogWarning("Invalid date format received. Start: '{StartParam}', End: '{EndParam}'", start, end);
                // Return ProblemDetails for better client info on 400
                return BadRequest(new ProblemDetails { Title = "Invalid date format provided.", Status = StatusCodes.Status400BadRequest });
            }

            _logger.LogInformation("Parsed dates successfully. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

            try
            {
                var shifts = await _apiClient.GetShiftsAsync(startDate, endDate, employeeId, teamId, isOnCall);
                _logger.LogInformation("Successfully retrieved {ShiftCount} shifts from ApiClientService.", shifts?.Count() ?? 0);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying shifts request."); // Log full exception
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Failed to retrieve shift data.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet("leaverequests")]
        public async Task<IActionResult> GetLeaveRequestsForCalendar(
            [FromQuery] string start, [FromQuery] string end,
            [FromQuery] int? employeeId = null, [FromQuery] int? teamId = null, [FromQuery] int? leaveTypeId = null,
            [FromQuery] LeaveStatus? status = LeaveStatus.Approved) // Keep default
        {
            _logger.LogInformation("Proxy GetLeaveRequestsForCalendar called. StartParam: '{StartParam}', EndParam: '{EndParam}'", start, end);

            if (!DateTime.TryParse(start, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime startDate) ||
                !DateTime.TryParse(end, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime endDate))
            {
                _logger.LogWarning("Invalid date format received. Start: '{StartParam}', End: '{EndParam}'", start, end);
                return BadRequest(new ProblemDetails { Title = "Invalid date format provided.", Status = StatusCodes.Status400BadRequest });
            }

            _logger.LogInformation("Parsed dates successfully. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

            try
            {
                var leaveRequests = await _apiClient.GetLeaveRequestsAsync(startDate, endDate, employeeId, teamId, leaveTypeId, LeaveStatus.Approved); // Force approved
                _logger.LogInformation("Successfully retrieved {LeaveCount} leave requests from ApiClientService.", leaveRequests?.Count() ?? 0);
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying leave requests request.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Failed to retrieve leave request data.", Status = StatusCodes.Status500InternalServerError });
            }
        }
    }
}