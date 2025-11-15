using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class MaintenanceType
{
    public int MtypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
}
