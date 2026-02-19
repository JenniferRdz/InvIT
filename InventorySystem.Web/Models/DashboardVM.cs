using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Models
{
    public class DashboardVM
    {
        public int TotalEquipos { get; set; }

        public int EquiposActivos { get; set; }
        public int EquiposBaja { get; set; }
        public int EquiposEnReparacion { get; set; }

        public int Win11Actualizado { get; set; }
        public int Win11EnProceso { get; set; }
        public int Win11NoCumple { get; set; }

        public List<DashboardScheduleRow> Proximos { get; set; } = new();
    }

}