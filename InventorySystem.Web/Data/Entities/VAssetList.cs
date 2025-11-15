using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class VAssetList
{
    public int AssetId { get; set; }

    public string AssetTag { get; set; } = null!;

    public string? Tipo { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string? Ubicacion { get; set; }

    public string? Responsable { get; set; }

    public string? Win11 { get; set; }

    public string? EstadoEquipo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
