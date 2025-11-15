using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class Asset
{
    public int AssetId { get; set; }

    public string AssetTag { get; set; } = null!;

    public int? AssetTypeId { get; set; }

    public int? BrandId { get; set; }

    public int? ModelId { get; set; }

    public string? ModelText { get; set; }

    public string SerialNumber { get; set; } = null!;

    public int? LocationId { get; set; }

    public string? Assignee { get; set; }

    public int? Win11StatusId { get; set; }

    public string? Comments { get; set; }

    public int EqStatusId { get; set; }

    public DateOnly? DecommissionDate { get; set; }

    public int? DecommissionReasonId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AssetType? AssetType { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual DecommissionReason? DecommissionReason { get; set; }

    public virtual EquipmentStatus EqStatus { get; set; } = null!;

    public virtual Location? Location { get; set; }

    public virtual ModelCatalog? Model { get; set; }

    public virtual Win11Status? Win11Status { get; set; }
}
