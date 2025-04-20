using System.ComponentModel.DataAnnotations;

namespace ROTA_MVC.Models
{
    public class UpdateTeamDto
    {
        [Required]
        [MaxLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
