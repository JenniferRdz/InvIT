using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class MaintenanceSchedule
{
    public int ScheduleId { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string Frequency { get; set; } = null!;

    public DateOnly? LastDone { get;  set; }

    public DateOnly NextDue { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
