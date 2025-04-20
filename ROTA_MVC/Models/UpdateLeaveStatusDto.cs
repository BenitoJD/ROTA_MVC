using System.ComponentModel.DataAnnotations;

namespace ROTA_MVC.Models
{
    public class UpdateLeaveStatusDto
    {
        [Required]
        // Ensure only valid target statuses are allowed (Approved/Rejected)
        [EnumDataType(typeof(LeaveStatus))]
        public LeaveStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? ApproverNotes { get; set; }
    }
}
