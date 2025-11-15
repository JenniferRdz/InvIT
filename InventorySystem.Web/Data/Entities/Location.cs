using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class Location
{
    public int LocationId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<Peripheral> Peripherals { get; set; } = new List<Peripheral>();
}
