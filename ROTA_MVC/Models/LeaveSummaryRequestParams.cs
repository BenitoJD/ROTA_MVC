namespace ROTA_MVC.Models
{
    public enum LeaveSummaryGrouping
    {
        None, // Overall summary
        LeaveType,
        Team,
        Employee
    }

    // Class to hold the request parameters for the Leave Summary API call
    public class LeaveSummaryRequestParams
    {
        public DateTime? StartDate { get; set; } // Overall range start
        public DateTime? EndDate { get; set; }   // Overall range end

        // Optional Filters
        public int? TeamId { get; set; }      // Filter results to a specific team
        public int? EmployeeId { get; set; }  // Filter results to a specific employee
        public int? LeaveTypeId { get; set; } // Filter results to a specific leave type

        // Grouping Option
        public LeaveSummaryGrouping GroupBy { get; set; } = LeaveSummaryGrouping.LeaveType; // Default grouping
    }
}
