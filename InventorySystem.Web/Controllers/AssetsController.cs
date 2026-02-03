using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Models; // ✅ para AssetFormViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // ✅ SelectListItem
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AssetsController : Controller
    {
        private readonly InventoryContext _db;

        public AssetsController(InventoryContext db)
        {
            _db = db;
        }

        // ===========================
        // ViewModels internos (se quedan para Index/Edit/Decommission)
        // ===========================
        public class AssetListVM
        {
            public int AssetId { get; set; }
            public string AssetTag { get; set; } = "";
            public string Tipo { get; set; } = "";
            public string Marca { get; set; } = "";
            public string Modelo { get; set; } = "";
            public string Serie { get; set; } = "";
            public string Ubicacion { get; set; } = "";
            public string Responsable { get; set; } = "";
            public string Win11 { get; set; } = "";
            public string Estado { get; set; } = "";
        }

        public class AssetEditVM
        {
            public int AssetId { get; set; }

            public string AssetTag { get; set; } = "";

            public int? AssetTypeId { get; set; }
            public int? BrandId { get; set; }
            public int? ModelId { get; set; }
            public string? ModelText { get; set; }

            public string SerialNumber { get; set; } = "";

            public int? LocationId { get; set; }
            public string? Assignee { get; set; }

            public int? Win11StatusId { get; set; }

            // Estado lógico del equipo (Activo / En reparación / Baja)
            public int? EqStatusId { get; set; }

            public string? Comments { get; set; }

            // Catálogos para combos
            public IEnumerable<AssetType>? Types { get; set; }
            public IEnumerable<Brand>? Brands { get; set; }
            public IEnumerable<ModelCatalog>? Models { get; set; }
            public IEnumerable<Location>? Locations { get; set; }
            public IEnumerable<EquipmentStatus>? Statuses { get; set; }
            public IEnumerable<Win11Status>? Win11Statuses { get; set; }
        }

        public class AssetDecommissionVM
        {
            public int AssetId { get; set; }
            public string AssetTag { get; set; } = "";
            public string Tipo { get; set; } = "";
            public string Marca { get; set; } = "";
            public string Modelo { get; set; } = "";
            public string Serie { get; set; } = "";
            public string? Responsable { get; set; }
            public string? Ubicacion { get; set; }

            public int? ReasonId { get; set; }
            public DateOnly? FechaBaja { get; set; }
            public DateOnly? FechaSugerida { get; set; }

            public IEnumerable<DecommissionReason>? Reasons { get; set; }
        }

        // ===========================
        // Helpers catálogos (para EDIT)
        // ===========================
        private async Task LoadCatalogsAsync(AssetEditVM vm)
        {
            vm.Types = await _db.AssetTypes.OrderBy(x => x.Name).ToListAsync();
            vm.Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync();
            vm.Models = await _db.ModelCatalogs.OrderBy(x => x.Name).ToListAsync();
            vm.Locations = await _db.Locations.OrderBy(x => x.Name).ToListAsync();
            vm.Statuses = await _db.EquipmentStatuses.OrderBy(x => x.Name).ToListAsync();
            vm.Win11Statuses = await _db.Win11Statuses.OrderBy(x => x.Name).ToListAsync();
        }

        // ✅ Helpers catálogos (para CREATE con AssetFormViewModel)
        private async Task LoadCatalogsAsync(AssetFormViewModel vm)
        {
            vm.AssetTypes = await _db.AssetTypes
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Value = x.AssetTypeId.ToString(), Text = x.Name })
                .ToListAsync();

            vm.Brands = await _db.Brands
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Value = x.BrandId.ToString(), Text = x.Name })
                .ToListAsync();

            vm.Locations = await _db.Locations
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Value = x.LocationId.ToString(), Text = x.Name })
                .ToListAsync();

            vm.Win11Statuses = await _db.Win11Statuses
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem { Value = x.Win11StatusId.ToString(), Text = x.Name })
                .ToListAsync();
        }

        // ===========================
        // INDEX (lista + filtros)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Index(string? q, int? locationId, int? typeId, int? statusId, int? win11Id)
        {
            ViewBag.Locations = await _db.Locations.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Types = await _db.AssetTypes.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Status = await _db.EquipmentStatuses.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Win11 = await _db.Win11Statuses.OrderBy(x => x.Name).ToListAsync();

            var query = _db.Assets
                .Include(a => a.AssetType)
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Location)
                .Include(a => a.Win11Status)
                .Include(a => a.EqStatus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim();
                query = query.Where(a =>
                    a.AssetTag.Contains(k) ||
                    a.SerialNumber.Contains(k) ||
                    (a.ModelText != null && a.ModelText.Contains(k)) ||
                    (a.Assignee != null && a.Assignee.Contains(k)));
            }

            if (locationId.HasValue)
                query = query.Where(a => a.LocationId == locationId);

            if (typeId.HasValue)
                query = query.Where(a => a.AssetTypeId == typeId);

            if (statusId.HasValue)
                query = query.Where(a => a.EqStatusId == statusId);

            if (win11Id.HasValue)
                query = query.Where(a => a.Win11StatusId == win11Id);

            var list = await query
                .OrderBy(a => a.AssetTag)
                .Select(a => new AssetListVM
                {
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    Tipo = a.AssetType != null ? a.AssetType.Name : "",
                    Marca = a.Brand != null ? a.Brand.Name : "",
                    Modelo = a.Model != null ? a.Model.Name : (a.ModelText ?? ""),
                    Serie = a.SerialNumber,
                    Ubicacion = a.Location != null ? a.Location.Name : "",
                    Responsable = a.Assignee ?? "",
                    Win11 = a.Win11Status != null ? a.Win11Status.Name : "",
                    Estado = a.EqStatus != null ? a.EqStatus.Name : ""
                })
                .ToListAsync();

            ViewBag.FQ = q;
            ViewBag.FLoc = locationId;
            ViewBag.FType = typeId;
            ViewBag.FSta = statusId;
            ViewBag.FW11 = win11Id;

            return View(list);
        }

        // ===========================
        // DETALLES
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var asset = await _db.Assets
                .Include(a => a.AssetType)
                .Include(a => a.Brand)
                .Include(a => a.Model)
                .Include(a => a.Location)
                .Include(a => a.Win11Status)
                .Include(a => a.EqStatus)
                .Include(a => a.DecommissionReason)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
                return NotFound();

            return View(asset);
        }

        // ===========================
        // ✅ CREAR (CORREGIDO PARA TU VISTA AssetFormViewModel)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new AssetFormViewModel();
            await LoadCatalogsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadCatalogsAsync(vm);
                return View(vm);
            }

            // (Opcional) evitar duplicado AssetTag
            var exists = await _db.Assets.AnyAsync(a => a.AssetTag == vm.AssetTag.Trim());
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.AssetTag), "Ya existe un equipo con ese Asset Tag.");
                await LoadCatalogsAsync(vm);
                return View(vm);
            }

            // Estado por defecto "Activo" si existe
            var activoId = await _db.EquipmentStatuses
                .Where(e => e.Name == "Activo")
                .Select(e => (int?)e.EqStatusId)
                .FirstOrDefaultAsync();

            if (!activoId.HasValue)
            {
                // Si no existe "Activo" en catálogo, evita crash
                ModelState.AddModelError("", "No existe el estado 'Activo' en el catálogo EquipmentStatuses.");
                await LoadCatalogsAsync(vm);
                return View(vm);
            }

            var entity = new Asset
            {
                AssetTag = vm.AssetTag.Trim(),
                AssetTypeId = vm.AssetTypeId,
                BrandId = vm.BrandId,
                // En tu vista usas ModelText (texto), no usas ModelCatalog
                ModelId = null,
                ModelText = string.IsNullOrWhiteSpace(vm.ModelText) ? null : vm.ModelText.Trim(),
                SerialNumber = string.IsNullOrWhiteSpace(vm.SerialNumber) ? "" : vm.SerialNumber.Trim(),
                LocationId = vm.LocationId,
                Assignee = string.IsNullOrWhiteSpace(vm.Assignee) ? null : vm.Assignee.Trim(),
                Win11StatusId = vm.Win11StatusId,
                EqStatusId = activoId.Value,
                Comments = string.IsNullOrWhiteSpace(vm.Comments) ? null : vm.Comments.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Assets.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Equipo creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // EDITAR
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var a = await _db.Assets.FindAsync(id);
            if (a == null) return NotFound();

            var vm = new AssetEditVM
            {
                AssetId = a.AssetId,
                AssetTag = a.AssetTag,
                AssetTypeId = a.AssetTypeId,
                BrandId = a.BrandId,
                ModelId = a.ModelId,
                ModelText = a.ModelText,
                SerialNumber = a.SerialNumber,
                LocationId = a.LocationId,
                Assignee = a.Assignee,
                Win11StatusId = a.Win11StatusId,
                EqStatusId = a.EqStatusId,
                Comments = a.Comments
            };

            await LoadCatalogsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetEditVM vm)
        {
            if (id != vm.AssetId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadCatalogsAsync(vm);
                return View(vm);
            }

            if (!vm.EqStatusId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.EqStatusId), "Debes seleccionar un estado del equipo.");
                await LoadCatalogsAsync(vm);
                return View(vm);
            }

            var a = await _db.Assets.FindAsync(id);
            if (a == null) return NotFound();

            a.AssetTag = vm.AssetTag.Trim();
            a.AssetTypeId = vm.AssetTypeId;
            a.BrandId = vm.BrandId;
            a.ModelId = vm.ModelId;
            a.ModelText = string.IsNullOrWhiteSpace(vm.ModelText) ? null : vm.ModelText.Trim();
            a.SerialNumber = vm.SerialNumber.Trim();
            a.LocationId = vm.LocationId;
            a.Assignee = string.IsNullOrWhiteSpace(vm.Assignee) ? null : vm.Assignee.Trim();
            a.Win11StatusId = vm.Win11StatusId;
            a.EqStatusId = vm.EqStatusId.Value;
            a.Comments = string.IsNullOrWhiteSpace(vm.Comments) ? null : vm.Comments.Trim();
            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Equipo actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // BAJA LÓGICA (DECOMMISSION)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Decommission(int id)
        {
            var a = await _db.Assets
                .Include(x => x.AssetType)
                .Include(x => x.Brand)
                .Include(x => x.Model)
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.AssetId == id);

            if (a == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var vm = new AssetDecommissionVM
            {
                AssetId = a.AssetId,
                AssetTag = a.AssetTag,
                Tipo = a.AssetType?.Name ?? "",
                Marca = a.Brand?.Name ?? "",
                Modelo = a.Model != null ? a.Model.Name : (a.ModelText ?? ""),
                Serie = a.SerialNumber,
                Responsable = a.Assignee,
                Ubicacion = a.Location?.Name ?? "",
                ReasonId = a.DecommissionReasonId,
                FechaBaja = a.DecommissionDate ?? today,
                FechaSugerida = a.DecommissionDate ?? today,
                Reasons = await _db.DecommissionReasons.OrderBy(r => r.Name).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decommission(AssetDecommissionVM vm)
        {
            var a = await _db.Assets.FindAsync(vm.AssetId);
            if (a == null) return NotFound();

            if (!vm.ReasonId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.ReasonId), "Debes seleccionar un motivo de baja.");
                vm.Reasons = await _db.DecommissionReasons.OrderBy(r => r.Name).ToListAsync();
                return View(vm);
            }

            var bajaId = await _db.EquipmentStatuses
                .Where(e => e.Name == "Baja")
                .Select(e => e.EqStatusId)
                .FirstOrDefaultAsync();

            a.EqStatusId = bajaId;
            a.DecommissionReasonId = vm.ReasonId;
            a.DecommissionDate = vm.FechaBaja ?? DateOnly.FromDateTime(DateTime.Today);
            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Equipo dado de baja correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}