﻿namespace ROTA_MVC.Models
{
    public class TeamDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? Description { get; set; }
         public int EmployeeCount { get; set; }
    }
}
