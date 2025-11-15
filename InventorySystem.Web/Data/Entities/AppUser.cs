using System;
using System.Collections.Generic;

namespace InventorySystem.Web.Data.Entities;

public partial class AppUser
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[]? PasswordSalt { get; set; }

    public bool MustChangePassword { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public DateTime? LastPasswordChange { get; set; }

    public virtual ICollection<InternalNotice> InternalNotices { get; set; } = new List<InternalNotice>();
}
