using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class Maintenance
{
    public int MaintenanceId { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public int MtypeId { get; set; }

    public DateOnly PerformedOn { get; set; }

    public string? PerformedBy { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MaintenanceType Mtype { get; set; } = null!;
}
