using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Web.Models
{
    public class AssetFormViewModel
    {
        public int? AssetId { get; set; }   // para futuro Edit

        [Required]
        [Display(Name = "Número de inventario (Tag)")]
        public string AssetTag { get; set; } = string.Empty;

        [Display(Name = "Tipo de equipo")]
        public int? AssetTypeId { get; set; }

        [Display(Name = "Marca")]
        public int? BrandId { get; set; }

        [Display(Name = "Modelo (texto libre)")]
        public string? ModelText { get; set; }

        [Required]
        [Display(Name = "Número de serie")]
        public string SerialNumber { get; set; } = string.Empty;

        [Display(Name = "Ubicación")]
        public int? LocationId { get; set; }

        [Display(Name = "Responsable")]
        public string? Assignee { get; set; }

        [Display(Name = "Estado Windows 11")]
        public int? Win11StatusId { get; set; }

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }

        // Combos
        public List<SelectListItem> AssetTypes { get; set; } = new();
        public List<SelectListItem> Brands { get; set; } = new();
        public List<SelectListItem> Locations { get; set; } = new();
        public List<SelectListItem> Win11Statuses { get; set; } = new();
    }
}
