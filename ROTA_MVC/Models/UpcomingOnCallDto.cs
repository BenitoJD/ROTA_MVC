namespace ROTA_MVC.Models
{
    public class UpcomingOnCallDto
    {
        public DateTime Date { get; set; } // The specific date
        public List<OnCallAssignmentDto> Assignments { get; set; } = new();
    }
}
