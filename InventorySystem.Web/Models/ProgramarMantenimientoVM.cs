using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Web.Models
{
    public class ProgramarMantenimientoVM
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = "";
        public string? Modelo { get; set; }

        [Required(ErrorMessage = "Selecciona una fecha.")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        // Preventivo / Correctivo
        [Required(ErrorMessage = "Selecciona el tipo.")]
        public string Tipo { get; set; } = "Preventivo";

        // Preventivo: 3 o 6
        public int FrecuenciaMeses { get; set; } = 3;

        public int MtypeId { get; set; }

        public List<SelectListItem> Types { get; set; } = new();
        public string? Notes { get; set; }
    }
}
