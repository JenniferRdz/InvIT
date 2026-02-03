using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Web.Models
{
    public class AssetFormViewModel
    {
        // Campos del formulario
        [Required(ErrorMessage = "El Asset Tag es obligatorio.")]
        [Display(Name = "Asset Tag")]
        public string AssetTag { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un tipo de equipo.")]
        [Display(Name = "Tipo de equipo")]
        public int? AssetTypeId { get; set; }

        [Required(ErrorMessage = "Selecciona una marca.")]
        [Display(Name = "Marca")]
        public int? BrandId { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [Display(Name = "Modelo")]
        public string ModelText { get; set; } = string.Empty;

        [Display(Name = "Número de serie")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "Selecciona una ubicación.")]
        [Display(Name = "Ubicación")]
        public int? LocationId { get; set; }

        [Required(ErrorMessage = "El responsable es obligatorio.")]
        [Display(Name = "Responsable")]
        public string Assignee { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona el estatus de Windows 11.")]
        [Display(Name = "Windows 11")]
        public int? Win11StatusId { get; set; }

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }

        // Combos (listas)
        public IEnumerable<SelectListItem> AssetTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Brands { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Win11Statuses { get; set; } = new List<SelectListItem>();
    }
}