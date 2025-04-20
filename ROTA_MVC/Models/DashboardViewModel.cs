namespace ROTA_MVC.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<PendingCountDto> PendingLeaveCounts { get; set; } = Enumerable.Empty<PendingCountDto>();

        public IEnumerable<UpcomingOnCallDto> UpcomingOnCall { get; set; } = Enumerable.Empty<UpcomingOnCallDto>();

        // Store the date range used for the on-call display
        public DateTime OnCallStartDate { get; set; }
        public DateTime OnCallEndDate { get; set; }

        public IEnumerable<LeaveSummaryDto> LeaveSummaryByType { get; set; } = Enumerable.Empty<LeaveSummaryDto>();
        public IEnumerable<ShiftTypeDistributionDto> ShiftTypeDistribution { get; set; } = Enumerable.Empty<ShiftTypeDistributionDto>();
        public DateTime SummaryPeriodStartDate { get; set; } // Store date range used for summaries
        public DateTime SummaryPeriodEndDate { get; set; }
    }
}
