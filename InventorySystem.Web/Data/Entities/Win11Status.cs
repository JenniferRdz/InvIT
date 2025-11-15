using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class Win11Status
{
    public int Win11StatusId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
