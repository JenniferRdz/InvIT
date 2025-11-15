using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class Peripheral
{
    public int PeripheralId { get; set; }

    public string Category { get; set; } = null!;

    public int? BrandId { get; set; }

    public int? ModelId { get; set; }

    public string? ModelText { get; set; }

    public string SerialNumber { get; set; } = null!;

    public int? LocationId { get; set; }

    public string? Responsible { get; set; }

    public int? ConditionId { get; set; }

    public int EqStatusId { get; set; }

    public string? IpAddress { get; set; }

    public string? TonerModel { get; set; }

    public decimal? SizeInches { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual PeripheralCondition? Condition { get; set; }

    public virtual EquipmentStatus EqStatus { get; set; } = null!;

    public virtual Location? Location { get; set; }

    public virtual ModelCatalog? Model { get; set; }
}
