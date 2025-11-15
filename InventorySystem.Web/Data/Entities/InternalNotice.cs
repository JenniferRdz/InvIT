using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class InternalNotice
{
    public int NoticeId { get; set; }

    public int UserId { get; set; }

    public string Category { get; set; } = null!;

    public string? Priority { get; set; }

    public string Description { get; set; } = null!;

    public string? AssetTag { get; set; }

    public string? SerialNumber { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
