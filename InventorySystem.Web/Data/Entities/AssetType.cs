using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class AssetType
{
    public int AssetTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
