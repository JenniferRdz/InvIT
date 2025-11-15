using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class DecommissionReason
{
    public int ReasonId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
