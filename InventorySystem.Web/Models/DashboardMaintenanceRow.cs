using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Models
{

    public class DashboardScheduleRow
    {
        public int ScheduleId { get; set; }
        public int AssetId { get; set; }

        public DateTime NextDue { get; set; }
        public DateTime? LastDone { get; set; }
        public int FrecuenciaMeses { get; set; }

        public string AssetTag { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public string Responsable { get; set; } = "";

        public string Alerta { get; set; } = "OK";
    }
}
