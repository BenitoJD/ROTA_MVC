using System.ComponentModel.DataAnnotations;

namespace ROTA_MVC.Models
{
    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; } // The new active status
    }
}
