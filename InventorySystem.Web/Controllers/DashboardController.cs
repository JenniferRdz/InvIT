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
        // DASHBOARD (INDEX) 
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var vm = new DashboardVM();

            vm.TotalEquipos = await _db.Assets.CountAsync();

            var activoId = await _db.EquipmentStatuses
                .Where(x => x.Name == "Activo")
                .Select(x => (int?)x.EqStatusId)
                .FirstOrDefaultAsync();

            var bajaId = await _db.EquipmentStatuses
                .Where(x => x.Name == "Baja")
                .Select(x => (int?)x.EqStatusId)
                .FirstOrDefaultAsync();

            var repId = await _db.EquipmentStatuses
                .Where(x => x.Name == "En reparación" || x.Name == "En reparacion")
                .Select(x => (int?)x.EqStatusId)
                .FirstOrDefaultAsync();

            if (activoId.HasValue) vm.EquiposActivos = await _db.Assets.CountAsync(a => a.EqStatusId == activoId.Value);
            if (bajaId.HasValue) vm.EquiposBaja = await _db.Assets.CountAsync(a => a.EqStatusId == bajaId.Value);
            if (repId.HasValue) vm.EquiposEnReparacion = await _db.Assets.CountAsync(a => a.EqStatusId == repId.Value);

            var w11Act = await _db.Win11Statuses
                .Where(x => x.Name == "Actualizado")
                .Select(x => (int?)x.Win11StatusId)
                .FirstOrDefaultAsync();

            var w11Proc = await _db.Win11Statuses
                .Where(x => x.Name == "En proceso")
                .Select(x => (int?)x.Win11StatusId)
                .FirstOrDefaultAsync();

            var w11No = await _db.Win11Statuses
                .Where(x => x.Name == "No cumple")
                .Select(x => (int?)x.Win11StatusId)
                .FirstOrDefaultAsync();

            if (w11Act.HasValue) vm.Win11Actualizado = await _db.Assets.CountAsync(a => a.Win11StatusId == w11Act.Value);
            if (w11Proc.HasValue) vm.Win11EnProceso = await _db.Assets.CountAsync(a => a.Win11StatusId == w11Proc.Value);
            if (w11No.HasValue) vm.Win11NoCumple = await _db.Assets.CountAsync(a => a.Win11StatusId == w11No.Value);

            // Próximos mantenimientos (30 días) + vencidos 
            var today = DateOnly.FromDateTime(DateTime.Today);
            var limit = today.AddDays(30);

            var baseQuery =
                from s in _db.MaintenanceSchedules.AsNoTracking()
                join a in _db.Assets.AsNoTracking()
                    .Include(x => x.Location)
                    .Include(x => x.Model)
                on s.EntityId equals a.AssetId
                where s.EntityType == "Asset"
                      && s.NextDue <= limit
                select new
                {
                    s.ScheduleId,
                    s.EntityId,
                    s.LastDone,     
                    s.NextDue,      
                    s.Frequency,
                    a.AssetId,
                    a.AssetTag,
                    Modelo = a.Model != null ? a.Model.Name : (a.ModelText ?? ""),
                    Ubicacion = a.Location != null ? a.Location.Name : "",
                    Responsable = a.Assignee ?? ""
                };

            var total = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var pageRows = await baseQuery
                .OrderBy(x => x.NextDue) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();         

            vm.Proximos = pageRows.Select(x =>
            {
                var freq = ParseFrequencyMonths(x.Frequency);

                return new DashboardScheduleRow
                {
                    ScheduleId = x.ScheduleId,
                    AssetId = x.AssetId,
                    AssetTag = x.AssetTag,
                    Modelo = x.Modelo,
                    Ubicacion = x.Ubicacion,
                    Responsable = x.Responsable,
                    FrecuenciaMeses = freq,

                    LastDone = x.LastDone.HasValue
                        ? x.LastDone.Value.ToDateTime(TimeOnly.MinValue)
                        : (DateTime?)null,

                    NextDue = x.NextDue.ToDateTime(TimeOnly.MinValue),

                    Alerta = (x.NextDue < today) ? "OVERDUE"
                          : (x.NextDue <= today.AddDays(7)) ? "SOON"
                          : "OK"
                };
            }).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            return View(vm);
        }

        // MARCAR COMO HECHO (GET)
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

            var months = ParseFrequencyMonths(sched.Frequency);

            var vm = new MarkDoneVM
            {
                ScheduleId = sched.ScheduleId,
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                Modelo = asset.Model?.Name ?? asset.ModelText ?? "",
                Ubicacion = asset.Location?.Name ?? "",
                FrecuenciaMeses = months,
                FechaRealizada = DateTime.Today
            };

            await LoadMaintenanceTypes(vm);
            return View(vm);
        }

        // MARCAR COMO HECHO (POST)
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

            // Frecuencia por seguridad: solo 3 o 6
            var freqMonths = (vm.FrecuenciaMeses == 6) ? 6 : 3;

            // 1) Guardar historial
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
            sched.Status = "OK";

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Mantenimiento marcado como hecho y guardado en historial.";
            return RedirectToAction(nameof(Index));
        }

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

        private static int ParseFrequencyMonths(string? frequency)
        {
            if (string.IsNullOrWhiteSpace(frequency)) return 3; 

            var f = frequency.Trim().ToUpperInvariant();
            if (f.EndsWith("M") && int.TryParse(f[..^1], out var months))
                return months;

            return 3;
        }
    }
}
