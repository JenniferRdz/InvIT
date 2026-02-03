using System;
using System.Linq;
using System.Threading.Tasks;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly InventoryContext _db;

        public DashboardController(InventoryContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVM();

            vm.TotalEquipos = await _db.Assets.CountAsync();

            var activoId = await _db.EquipmentStatuses.Where(x => x.Name == "Activo")
                .Select(x => (int?)x.EqStatusId).FirstOrDefaultAsync();
            var bajaId = await _db.EquipmentStatuses.Where(x => x.Name == "Baja")
                .Select(x => (int?)x.EqStatusId).FirstOrDefaultAsync();
            var repId = await _db.EquipmentStatuses
                .Where(x => x.Name == "En reparación" || x.Name == "En reparacion")
                .Select(x => (int?)x.EqStatusId).FirstOrDefaultAsync();

            if (activoId.HasValue) vm.EquiposActivos = await _db.Assets.CountAsync(a => a.EqStatusId == activoId.Value);
            if (bajaId.HasValue) vm.EquiposBaja = await _db.Assets.CountAsync(a => a.EqStatusId == bajaId.Value);
            if (repId.HasValue) vm.EquiposEnReparacion = await _db.Assets.CountAsync(a => a.EqStatusId == repId.Value);

            var w11Act = await _db.Win11Statuses.Where(x => x.Name == "Actualizado")
                .Select(x => (int?)x.Win11StatusId).FirstOrDefaultAsync();
            var w11Proc = await _db.Win11Statuses.Where(x => x.Name == "En proceso")
                .Select(x => (int?)x.Win11StatusId).FirstOrDefaultAsync();
            var w11No = await _db.Win11Statuses.Where(x => x.Name == "No cumple")
                .Select(x => (int?)x.Win11StatusId).FirstOrDefaultAsync();

            if (w11Act.HasValue) vm.Win11Actualizado = await _db.Assets.CountAsync(a => a.Win11StatusId == w11Act.Value);
            if (w11Proc.HasValue) vm.Win11EnProceso = await _db.Assets.CountAsync(a => a.Win11StatusId == w11Proc.Value);
            if (w11No.HasValue) vm.Win11NoCumple = await _db.Assets.CountAsync(a => a.Win11StatusId == w11No.Value);

            // ================================
            // Próximos 30 días + vencidos
            // status permitido por CHECK: OK | EN_CURSO | CANCELADO (según tu constraint)
            // ================================
            var today = DateOnly.FromDateTime(DateTime.Today);
            var limit = today.AddDays(30);

            var schedules = await _db.MaintenanceSchedules
                .AsNoTracking()
                .Where(s => s.EntityType == "Asset" && (s.NextDue <= limit || s.NextDue < today))
                .OrderBy(s => s.NextDue)
                .ToListAsync();

            var assetIds = schedules.Select(s => s.EntityId).Distinct().ToList();

            var assets = await _db.Assets
                .AsNoTracking()
                .Include(a => a.Location)
                .Include(a => a.Model)
                .Where(a => assetIds.Contains(a.AssetId))
                .ToListAsync();

            var assetById = assets.ToDictionary(a => a.AssetId, a => a);

            vm.Proximos = schedules.Select(s =>
            {
                var a = assetById.TryGetValue(s.EntityId, out var aa) ? aa : null;

                var months = ParseFrequencyMonths(s.Frequency) ?? 3;

                var nextDueDt = s.NextDue.ToDateTime(TimeOnly.MinValue);
                DateTime? lastDoneDt = s.LastDone.HasValue ? s.LastDone.Value.ToDateTime(TimeOnly.MinValue) : null;

                var alerta = (s.NextDue < today) ? "OVERDUE"
                          : (s.NextDue <= today.AddDays(7)) ? "SOON"
                          : "OK";

                return new DashboardScheduleRow
                {
                    ScheduleId = s.ScheduleId,
                    AssetId = s.EntityId,
                    NextDue = nextDueDt,
                    LastDone = lastDoneDt,
                    FrecuenciaMeses = months,

                    AssetTag = a?.AssetTag ?? "",
                    Modelo = a?.Model?.Name ?? (a?.ModelText ?? ""),
                    Ubicacion = a?.Location?.Name ?? "",
                    Responsable = a?.Assignee ?? "",
                    Alerta = alerta
                };
            }).ToList();

            return View(vm);
        }

        // ==================================
        // GET: Marcar como hecho
        // ==================================
        [HttpGet]
        public async Task<IActionResult> MarkDone(int id)
        {
            var sched = await _db.MaintenanceSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ScheduleId == id);

            if (sched == null) return NotFound();

            var asset = await _db.Assets
                .AsNoTracking()
                .Include(a => a.Location)
                .Include(a => a.Model)
                .FirstOrDefaultAsync(a => a.AssetId == sched.EntityId);

            if (asset == null) return NotFound();

            var months = ParseFrequencyMonths(sched.Frequency) ?? 3;

            var vm = new MarkDoneVM
            {
                ScheduleId = sched.ScheduleId,
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                Modelo = asset.Model?.Name ?? asset.ModelText,
                Ubicacion = asset.Location?.Name,
                FrecuenciaMeses = months,
                FechaRealizada = DateTime.Today
            };

            await LoadMaintenanceTypes(vm);
            return View(vm);
        }

        // ==================================
        // POST: Marcar como hecho
        // ==================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(MarkDoneVM vm)
        {
            await LoadMaintenanceTypes(vm);

            if (!ModelState.IsValid)
                return View(vm);

            if (vm.MtypeId <= 0)
            {
                ModelState.AddModelError(nameof(vm.MtypeId), "Selecciona el tipo de mantenimiento.");
                return View(vm);
            }

            var sched = await _db.MaintenanceSchedules
                .FirstOrDefaultAsync(s => s.ScheduleId == vm.ScheduleId);

            if (sched == null) return NotFound();

            if (vm.FechaRealizada == null)
            {
                ModelState.AddModelError(nameof(vm.FechaRealizada), "Selecciona la fecha realizada.");
                return View(vm);
            }

            var doneDate = DateOnly.FromDateTime(vm.FechaRealizada.Value);

            var freqMonths = (vm.FrecuenciaMeses == 6) ? 6 : 3;

            // 1) Guardar historial en Maintenance
            var m = new Maintenance
            {
                EntityType = "Asset",
                EntityId = sched.EntityId,
                MtypeId = vm.MtypeId,
                PerformedOn = doneDate,
                PerformedBy = User?.Identity?.Name,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            };
            _db.Maintenances.Add(m);

            // 2) Actualizar plan
            sched.LastDone = doneDate;
            sched.NextDue = doneDate.AddMonths(freqMonths);

            // OJO: tu CHECK constraint permite: OK, EN_CURSO, CANCELADO
            sched.Status = "OK";

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Mantenimiento marcado como hecho y guardado en historial.";
            return RedirectToAction(nameof(Index));
        }

        // ==================================
        // Helpers
        // ==================================
        private async Task LoadMaintenanceTypes(MarkDoneVM vm)
        {
            vm.Types = await _db.MaintenanceTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.MtypeId.ToString(),
                    Text = x.Name
                })
                .ToListAsync();
        }

        private int? ParseFrequencyMonths(string? frequency)
        {
            if (string.IsNullOrWhiteSpace(frequency)) return null;
            var f = frequency.Trim().ToUpperInvariant(); // "3M"
            if (!f.EndsWith("M")) return null;
            var num = f[..^1];
            return int.TryParse(num, out var m) ? m : null;
        }
    }
}
