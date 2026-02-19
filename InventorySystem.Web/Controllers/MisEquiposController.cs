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
    [Authorize]
    public class MisEquiposController : Controller
    {
        private readonly InventoryContext _db;

        private const string ENTITY_ASSET = "ASSET";

        public MisEquiposController(InventoryContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var soonLimit = today.AddDays(7);

            IQueryable<Asset> assetsQuery = _db.Assets
                .AsNoTracking()
                .Include(a => a.Location)
                .Include(a => a.Win11Status)
                .Include(a => a.EqStatus)
                .Include(a => a.Model);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim();
                assetsQuery = assetsQuery.Where(a =>
                    a.AssetTag.Contains(k) ||
                    a.SerialNumber.Contains(k) ||
                    (a.ModelText != null && a.ModelText.Contains(k)) ||
                    (a.Assignee != null && a.Assignee.Contains(k)));
            }

            var total = await assetsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var assets = await assetsQuery
                .OrderBy(a => a.AssetTag)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var assetIds = assets.Select(a => a.AssetId).ToList();

            var schedules = await _db.MaintenanceSchedules
                .AsNoTracking()
                .Where(s => s.EntityType == ENTITY_ASSET && assetIds.Contains(s.EntityId))
                .ToListAsync();

            var schedByAsset = schedules
                .GroupBy(s => s.EntityId)
                .ToDictionary(g => g.Key, g => g.First());

            var rows = assets.Select(a =>
            {
                DateOnly? lastDone = null;
                DateOnly? nextDue = null;

                if (schedByAsset.TryGetValue(a.AssetId, out var sched))
                {
                    lastDone = sched.LastDone;
                    nextDue = sched.NextDue;
                }

                string alerta;
                string alertaTexto;

                if (nextDue == null)
                {
                    alerta = "NONE";
                    alertaTexto = "Sin plan";
                }
                else if (nextDue.Value < today)
                {
                    alerta = "OVERDUE";
                    alertaTexto = "Vencido";
                }
                else if (nextDue.Value <= soonLimit)
                {
                    alerta = "SOON";
                    alertaTexto = "Próximo (7 días)";
                }
                else
                {
                    alerta = "OK";
                    alertaTexto = "En regla";
                }

                DateTime? lastDoneDt = lastDone.HasValue
                    ? lastDone.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null;

                DateTime? nextDueDt = nextDue.HasValue
                    ? nextDue.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null;

                return new MisEquipoRow
                {
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    Modelo = a.Model != null ? a.Model.Name : (a.ModelText ?? ""),
                    Serie = a.SerialNumber,
                    Ubicacion = a.Location != null ? a.Location.Name : "",
                    Responsable = a.Assignee ?? "",
                    Win11 = a.Win11Status != null ? a.Win11Status.Name : "",
                    Estado = a.EqStatus != null ? a.EqStatus.Name : "",

                    UltimoMantenimiento = lastDoneDt,
                    ProximoMantenimiento = nextDueDt,
                    Alerta = alerta,
                    AlertaTexto = alertaTexto
                };
            }).ToList();

            var vm = new MisEquiposIndexVM
            {
                Usuario = User?.Identity?.Name ?? "Usuario",
                Q = q,
                Equipos = rows
            };

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            return View(vm);
        }

        // PROGRAMAR MANTENIMIENTO
        [HttpGet]
        public async Task<IActionResult> ScheduleMaintenance(int id)
        {
            var asset = await _db.Assets
                .AsNoTracking()
                .Include(a => a.Model)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null) return NotFound();

            var vm = new ProgramarMantenimientoVM
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                Modelo = asset.Model != null ? asset.Model.Name : asset.ModelText,
                Date = DateTime.Today.AddDays(1),
                Tipo = "Preventivo",
                FrecuenciaMeses = 3,
                MtypeId = 0
            };

            await LoadMaintenanceTypes(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleMaintenance(ProgramarMantenimientoVM vm)
        {
            await LoadMaintenanceTypes(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var asset = await _db.Assets.FirstOrDefaultAsync(a => a.AssetId == vm.AssetId);
            if (asset == null) return NotFound();

            if (vm.Date == null)
            {
                ModelState.AddModelError(nameof(vm.Date), "Selecciona una fecha.");
                return View(vm);
            }

            var date = DateOnly.FromDateTime(vm.Date.Value);

            // PREVENTIVO: guarda/actualiza MaintenanceSchedule
            if (vm.Tipo == "Preventivo")
            {
                if (vm.FrecuenciaMeses != 3 && vm.FrecuenciaMeses != 6)
                {
                    ModelState.AddModelError(nameof(vm.FrecuenciaMeses), "Frecuencia válida: 3 o 6 meses.");
                    return View(vm);
                }

                var sched = await _db.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.EntityType == ENTITY_ASSET && s.EntityId == asset.AssetId);

                if (sched == null)
                {
                    sched = new MaintenanceSchedule
                    {
                        EntityType = ENTITY_ASSET,
                        EntityId = asset.AssetId,
                        Frequency = $"{vm.FrecuenciaMeses}M",
                        Status = "EN_CURSO",
                        LastDone = null,
                        NextDue = date,
                        CreatedAt = DateTime.Now
                    };
                    _db.MaintenanceSchedules.Add(sched);
                }
                else
                {
                    sched.Frequency = $"{vm.FrecuenciaMeses}M";
                    sched.Status = "EN_CURSO";
                    sched.NextDue = date;
                }

                await _db.SaveChangesAsync();
                TempData["Msg"] = "Mantenimiento preventivo programado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // CORRECTIVO: solo historial (Maintenance)
            if (vm.Tipo == "Correctivo")
            {
                if (vm.MtypeId <= 0)
                {
                    ModelState.AddModelError(nameof(vm.MtypeId), "Selecciona el tipo de mantenimiento.");
                    return View(vm);
                }

                var m = new Maintenance
                {
                    EntityType = ENTITY_ASSET,
                    EntityId = asset.AssetId,
                    MtypeId = vm.MtypeId,
                    PerformedOn = date,
                    PerformedBy = User?.Identity?.Name,
                    Notes = vm.Notes,
                    CreatedAt = DateTime.Now
                };

                _db.Maintenances.Add(m);
                await _db.SaveChangesAsync();

                TempData["Msg"] = "Mantenimiento correctivo registrado en historial.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(nameof(vm.Tipo), "Tipo inválido.");
            return View(vm);
        }

        // HISTORIAL
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var asset = await _db.Assets
                .AsNoTracking()
                .Include(a => a.Location)
                .Include(a => a.Model)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null) return NotFound();

            var items = await _db.Maintenances
                .AsNoTracking()
                .Include(m => m.Mtype)
                .Where(m => m.EntityType == ENTITY_ASSET && m.EntityId == id)
                .OrderByDescending(m => m.PerformedOn)
                .Select(m => new MaintenanceHistoryRow
                {
                    MaintenanceId = m.MaintenanceId,
                    PerformedOn = m.PerformedOn,
                    Tipo = m.Mtype.Name,
                    PerformedBy = m.PerformedBy,
                    Notes = m.Notes
                })
                .ToListAsync();

            var vm = new MaintenanceHistoryVM
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                Modelo = asset.Model != null ? asset.Model.Name : (asset.ModelText ?? ""),
                Serie = asset.SerialNumber,
                Ubicacion = asset.Location?.Name ?? "",
                Responsable = asset.Assignee ?? "",
                Items = items
            };

            return View(vm);
        }

        private async Task LoadMaintenanceTypes(ProgramarMantenimientoVM vm)
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
    }
}
