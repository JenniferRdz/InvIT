using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Web.Models
{
    public class MarkDoneVM
    {
        public int ScheduleId { get; set; }
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = "";
        public string? Modelo { get; set; }
        public string? Ubicacion { get; set; }
        public int FrecuenciaMeses { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? FechaRealizada { get; set; }

        [Required(ErrorMessage = "Selecciona el tipo de mantenimiento.")]
        public int MtypeId { get; set; } // <- INT (no nullable)

        public List<SelectListItem> Types { get; set; } = new();
        public string? Notes { get; set; }
    }
}
