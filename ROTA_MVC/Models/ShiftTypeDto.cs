﻿namespace ROTA_MVC.Models
{
    public class ShiftTypeDto
    {
        public int ShiftTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsOnCall { get; set; }
        public string? Description { get; set; }
    }
}
