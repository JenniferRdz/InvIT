using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Models
{
    public class MaintenanceHistoryVM
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string Serie { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public string Responsable { get; set; } = "";

        public List<MaintenanceHistoryRow> Items { get; set; } = new();
    }

    public class MaintenanceHistoryRow
    {
        public int MaintenanceId { get; set; }
        public DateOnly PerformedOn { get; set; }
        public string Tipo { get; set; } = "";
        public string? PerformedBy { get; set; }
        public string? Notes { get; set; }
    }
}
