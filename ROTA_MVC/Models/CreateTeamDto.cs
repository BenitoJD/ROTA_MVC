﻿using System.ComponentModel.DataAnnotations;

namespace ROTA_MVC.Models
{
    public class CreateTeamDto
    {
        [Required]
        [MaxLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [MaxLength(500)] // Max length for description
        public string? Description { get; set; }
    }
}
