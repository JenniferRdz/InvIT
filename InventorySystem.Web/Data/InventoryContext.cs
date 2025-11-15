using System;
using System.Collections.Generic;
using InventorySystem.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Web.Data;

public partial class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetType> AssetTypes { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<DecommissionReason> DecommissionReasons { get; set; }

    public virtual DbSet<EquipmentStatus> EquipmentStatuses { get; set; }

    public virtual DbSet<InternalNotice> InternalNotices { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Maintenance> Maintenances { get; set; }

    public virtual DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

    public virtual DbSet<MaintenanceType> MaintenanceTypes { get; set; }

    public virtual DbSet<ModelCatalog> ModelCatalogs { get; set; }

    public virtual DbSet<Peripheral> Peripherals { get; set; }

    public virtual DbSet<PeripheralCondition> PeripheralConditions { get; set; }

    public virtual DbSet<VAssetList> VAssetLists { get; set; }

    public virtual DbSet<Win11Status> Win11Statuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__AppUser__B9BE370FFDEE7075");

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.Email, "UQ__AppUser__AB6E6164A897EACC").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(180)
                .HasColumnName("email");
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");
            entity.Property(e => e.LastPasswordChange).HasColumnName("last_password_change");
            entity.Property(e => e.LockoutUntil).HasColumnName("lockout_until");
            entity.Property(e => e.MustChangePassword)
                .HasDefaultValue(true)
                .HasColumnName("must_change_password");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(64)
                .HasColumnName("password_salt");
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("role");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__Asset__D28B561D5377ACF1");

            entity.ToTable("Asset");

            entity.HasIndex(e => e.LocationId, "IX_Asset_Location");

            entity.HasIndex(e => e.EqStatusId, "IX_Asset_Status");

            entity.HasIndex(e => e.Win11StatusId, "IX_Asset_Win11");

            entity.HasIndex(e => e.AssetTag, "UQ__Asset__1FACF043610A5D76").IsUnique();

            entity.HasIndex(e => e.SerialNumber, "UQ__Asset__BED14FEE884F454C").IsUnique();

            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.AssetTag)
                .HasMaxLength(50)
                .HasColumnName("asset_tag");
            entity.Property(e => e.AssetTypeId).HasColumnName("asset_type_id");
            entity.Property(e => e.Assignee)
                .HasMaxLength(120)
                .HasColumnName("assignee");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Comments)
                .HasMaxLength(500)
                .HasColumnName("comments");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DecommissionDate).HasColumnName("decommission_date");
            entity.Property(e => e.DecommissionReasonId).HasColumnName("decommission_reason_id");
            entity.Property(e => e.EqStatusId).HasColumnName("eq_status_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.ModelText)
                .HasMaxLength(120)
                .HasColumnName("model_text");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(120)
                .HasColumnName("serial_number");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Win11StatusId).HasColumnName("win11_status_id");

            entity.HasOne(d => d.AssetType).WithMany(p => p.Assets)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK__Asset__asset_typ__59FA5E80");

            entity.HasOne(d => d.Brand).WithMany(p => p.Assets)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Asset__brand_id__5AEE82B9");

            entity.HasOne(d => d.DecommissionReason).WithMany(p => p.Assets)
                .HasForeignKey(d => d.DecommissionReasonId)
                .HasConstraintName("FK__Asset__decommiss__5FB337D6");

            entity.HasOne(d => d.EqStatus).WithMany(p => p.Assets)
                .HasForeignKey(d => d.EqStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Asset__eq_status__5EBF139D");

            entity.HasOne(d => d.Location).WithMany(p => p.Assets)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Asset__location___5CD6CB2B");

            entity.HasOne(d => d.Model).WithMany(p => p.Assets)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK__Asset__model_id__5BE2A6F2");

            entity.HasOne(d => d.Win11Status).WithMany(p => p.Assets)
                .HasForeignKey(d => d.Win11StatusId)
                .HasConstraintName("FK__Asset__win11_sta__5DCAEF64");
        });

        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.HasKey(e => e.AssetTypeId).HasName("PK__AssetTyp__95A1E2BCD97D79CA");

            entity.ToTable("AssetType");

            entity.HasIndex(e => e.Name, "UQ__AssetTyp__72E12F1B1382E7B8").IsUnique();

            entity.Property(e => e.AssetTypeId).HasColumnName("asset_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brand__5E5A8E27BAA74148");

            entity.ToTable("Brand");

            entity.HasIndex(e => e.Name, "UQ__Brand__72E12F1BB0D4EF28").IsUnique();

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DecommissionReason>(entity =>
        {
            entity.HasKey(e => e.ReasonId).HasName("PK__Decommis__846BB5546716DC04");

            entity.ToTable("DecommissionReason");

            entity.HasIndex(e => e.Name, "UQ__Decommis__72E12F1BA9F0A6E2").IsUnique();

            entity.Property(e => e.ReasonId).HasColumnName("reason_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<EquipmentStatus>(entity =>
        {
            entity.HasKey(e => e.EqStatusId).HasName("PK__Equipmen__D0DF5011C817EE11");

            entity.ToTable("EquipmentStatus");

            entity.HasIndex(e => e.Name, "UQ__Equipmen__72E12F1B680ADF91").IsUnique();

            entity.Property(e => e.EqStatusId).HasColumnName("eq_status_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<InternalNotice>(entity =>
        {
            entity.HasKey(e => e.NoticeId).HasName("PK__Internal__3E82A5DB087DDA61");

            entity.ToTable("InternalNotice");

            entity.Property(e => e.NoticeId).HasColumnName("notice_id");
            entity.Property(e => e.AssetTag)
                .HasMaxLength(50)
                .HasColumnName("asset_tag");
            entity.Property(e => e.Category)
                .HasMaxLength(60)
                .HasColumnName("category");
            entity.Property(e => e.ClosedAt).HasColumnName("closed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(800)
                .HasColumnName("description");
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("priority");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(120)
                .HasColumnName("serial_number");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasDefaultValue("ABIERTO")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.InternalNotices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InternalN__user___787EE5A0");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EA9DA68DD3");

            entity.ToTable("Location");

            entity.HasIndex(e => e.Name, "UQ__Location__72E12F1BAC82E5F0").IsUnique();

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId).HasName("PK__Maintena__9D754BEA8B11EC58");

            entity.ToTable("Maintenance");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.PerformedOn }, "IX_Maintenance_Entity").IsDescending(false, false, true);

            entity.Property(e => e.MaintenanceId).HasColumnName("maintenance_id");
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(260)
                .HasColumnName("attachment_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("entity_type");
            entity.Property(e => e.MtypeId).HasColumnName("mtype_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.PerformedBy)
                .HasMaxLength(120)
                .HasColumnName("performed_by");
            entity.Property(e => e.PerformedOn).HasColumnName("performed_on");

            entity.HasOne(d => d.Mtype).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.MtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__mtype__6FE99F9F");
        });

        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Maintena__C46A8A6FC2BFF935");

            entity.ToTable("MaintenanceSchedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("entity_type");
            entity.Property(e => e.Frequency)
                .HasMaxLength(30)
                .HasColumnName("frequency");
            entity.Property(e => e.LastDone).HasColumnName("last_done");
            entity.Property(e => e.NextDue).HasColumnName("next_due");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("status");
        });

        modelBuilder.Entity<MaintenanceType>(entity =>
        {
            entity.HasKey(e => e.MtypeId).HasName("PK__Maintena__86FE79897FDE48D3");

            entity.ToTable("MaintenanceType");

            entity.HasIndex(e => e.Name, "UQ__Maintena__72E12F1BBFB469AA").IsUnique();

            entity.Property(e => e.MtypeId).HasColumnName("mtype_id");
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ModelCatalog>(entity =>
        {
            entity.HasKey(e => e.ModelId).HasName("PK__ModelCat__DC39CAF40C3B0DAE");

            entity.ToTable("ModelCatalog");

            entity.HasIndex(e => new { e.BrandId, e.Name }, "UQ__ModelCat__E9749CD7FF48ABAC").IsUnique();

            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");

            entity.HasOne(d => d.Brand).WithMany(p => p.ModelCatalogs)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ModelCata__brand__46E78A0C");
        });

        modelBuilder.Entity<Peripheral>(entity =>
        {
            entity.HasKey(e => e.PeripheralId).HasName("PK__Peripher__5605F9EBD17081DC");

            entity.ToTable("Peripheral");

            entity.HasIndex(e => e.Category, "IX_Peripheral_Category");

            entity.HasIndex(e => e.LocationId, "IX_Peripheral_Location");

            entity.HasIndex(e => e.SerialNumber, "UQ__Peripher__BED14FEE52E3728C").IsUnique();

            entity.Property(e => e.PeripheralId).HasColumnName("peripheral_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Category)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.ConditionId).HasColumnName("condition_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EqStatusId).HasColumnName("eq_status_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.ModelText)
                .HasMaxLength(120)
                .HasColumnName("model_text");
            entity.Property(e => e.Responsible)
                .HasMaxLength(120)
                .HasColumnName("responsible");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(120)
                .HasColumnName("serial_number");
            entity.Property(e => e.SizeInches)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("size_inches");
            entity.Property(e => e.TonerModel)
                .HasMaxLength(80)
                .HasColumnName("toner_model");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Brand).WithMany(p => p.Peripherals)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Periphera__brand__66603565");

            entity.HasOne(d => d.Condition).WithMany(p => p.Peripherals)
                .HasForeignKey(d => d.ConditionId)
                .HasConstraintName("FK__Periphera__condi__693CA210");

            entity.HasOne(d => d.EqStatus).WithMany(p => p.Peripherals)
                .HasForeignKey(d => d.EqStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Periphera__eq_st__6A30C649");

            entity.HasOne(d => d.Location).WithMany(p => p.Peripherals)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Periphera__locat__68487DD7");

            entity.HasOne(d => d.Model).WithMany(p => p.Peripherals)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK__Periphera__model__6754599E");
        });

        modelBuilder.Entity<PeripheralCondition>(entity =>
        {
            entity.HasKey(e => e.ConditionId).HasName("PK__Peripher__8527AB1549A74ECB");

            entity.ToTable("PeripheralCondition");

            entity.HasIndex(e => e.Name, "UQ__Peripher__72E12F1B9D099F30").IsUnique();

            entity.Property(e => e.ConditionId).HasColumnName("condition_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<VAssetList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_AssetList");

            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.AssetTag)
                .HasMaxLength(50)
                .HasColumnName("asset_tag");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EstadoEquipo)
                .HasMaxLength(50)
                .HasColumnName("estado_equipo");
            entity.Property(e => e.Marca)
                .HasMaxLength(80)
                .HasColumnName("marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(120)
                .HasColumnName("modelo");
            entity.Property(e => e.Responsable)
                .HasMaxLength(120)
                .HasColumnName("responsable");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(120)
                .HasColumnName("serial_number");
            entity.Property(e => e.Tipo)
                .HasMaxLength(80)
                .HasColumnName("tipo");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(120)
                .HasColumnName("ubicacion");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Win11)
                .HasMaxLength(50)
                .HasColumnName("win11");
        });

        modelBuilder.Entity<Win11Status>(entity =>
        {
            entity.HasKey(e => e.Win11StatusId).HasName("PK__Win11Sta__AB2E068CDCCD396B");

            entity.ToTable("Win11Status");

            entity.HasIndex(e => e.Name, "UQ__Win11Sta__72E12F1B6BA02644").IsUnique();

            entity.Property(e => e.Win11StatusId).HasColumnName("win11_status_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
