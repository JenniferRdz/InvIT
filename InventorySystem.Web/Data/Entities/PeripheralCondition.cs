using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class PeripheralCondition
{
    public int ConditionId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Peripheral> Peripherals { get; set; } = new List<Peripheral>();
}
