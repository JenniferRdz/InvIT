using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Models
{
	public class MisEquiposIndexVM
	{
		public string Usuario { get; set; } = "";
		public string? Q { get; set; }
		public List<MisEquipoRow> Equipos { get; set; } = new();
	}

	public class MisEquipoRow
	{
		public int AssetId { get; set; }
		public string AssetTag { get; set; } = "";
		public string Modelo { get; set; } = "";
		public string Serie { get; set; } = "";
		public string Ubicacion { get; set; } = "";
		public string Responsable { get; set; } = "";
		public string Win11 { get; set; } = "";
		public string Estado { get; set; } = "";

		public DateTime? UltimoMantenimiento { get; set; }
		public DateTime? ProximoMantenimiento { get; set; }

		// "OK" | "SOON" | "OVERDUE" | "NONE"
		public string Alerta { get; set; } = "NONE";
		public string AlertaTexto { get; set; } = "Sin plan";
	}
}