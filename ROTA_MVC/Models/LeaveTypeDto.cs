namespace ROTA_MVC.Models
{
    public class LeaveTypeDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public string? Description { get; set; }
    }
}
