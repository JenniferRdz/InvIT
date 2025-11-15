using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class ModelCatalog
{
    public int ModelId { get; set; }

    public int BrandId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<Peripheral> Peripherals { get; set; } = new List<Peripheral>();
}
