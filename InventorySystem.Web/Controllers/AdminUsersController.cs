using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminUsersController : Controller
    {
        private readonly InventoryContext _db;
        public AdminUsersController(InventoryContext db) => _db = db;

        // GET: /AdminUsers
        public async Task<IActionResult> Index(string q = "", string role = "ALL", string state = "ALL", int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var query = _db.AppUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(u => (u.Name != null && u.Name.Contains(q)) ||
                                         (u.Email != null && u.Email.Contains(q)));

            if (role != "ALL")
                query = query.Where(u => u.Role == role);

            if (state == "ACTIVE") query = query.Where(u => u.Active);
            else if (state == "INACTIVE") query = query.Where(u => !u.Active);

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Q = q; ViewBag.Role = role; ViewBag.State = state;

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }


        // GET: /AdminUsers/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AppUser
            {
                Active = true,
                Role = "USER",
                MustChangePassword = true
            });
        }

        // POST: /AdminUsers/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser model, string? password)
        {
            ModelState.Remove(nameof(AppUser.PasswordHash));
            ModelState.Remove(nameof(AppUser.PasswordSalt));
            ModelState.Remove(nameof(AppUser.FailedLoginAttempts));
            ModelState.Remove(nameof(AppUser.LockoutUntil));
            ModelState.Remove(nameof(AppUser.LastPasswordChange));
            ModelState.Remove(nameof(AppUser.CreatedAt)); 

            if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError("", "Captura Email o Name (JER01).");

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (!PasswordPolicy.IsStrong(password, out var err))
                    ModelState.AddModelError("password", err);
            }

            if (!ModelState.IsValid)
                return View(model);

            string shownPassword;
            if (string.IsNullOrWhiteSpace(password))
            {
                shownPassword = GenerateTempPassword();
                model.MustChangePassword = true;
            }
            else
            {
                shownPassword = password;
            }

            // Hash + salt
            var (hash, salt) = PasswordHelper.Hash(shownPassword);
            model.PasswordHash = hash;
            model.PasswordSalt = salt;

            // Defaults de seguridad
            model.FailedLoginAttempts = 0;
            model.LockoutUntil = null;
            model.LastPasswordChange = null;

            _db.AppUsers.Add(model);
            await _db.SaveChangesAsync();

            TempData["ok"] = $"Usuario creado. Contraseña {(string.IsNullOrWhiteSpace(password) ? "temporal" : "asignada")}: {shownPassword}";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminUsers/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /AdminUsers/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUser input)
        {
            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            ModelState.Remove(nameof(AppUser.PasswordHash));
            ModelState.Remove(nameof(AppUser.PasswordSalt));
            ModelState.Remove(nameof(AppUser.FailedLoginAttempts));
            ModelState.Remove(nameof(AppUser.LockoutUntil));
            ModelState.Remove(nameof(AppUser.LastPasswordChange));
            ModelState.Remove(nameof(AppUser.CreatedAt));

            if (string.IsNullOrWhiteSpace(input.Email) && string.IsNullOrWhiteSpace(input.Name))
                ModelState.AddModelError("", "Captura Email o Name (JER01).");

            if (!ModelState.IsValid) return View(input);

            user.Name = input.Name;
            user.Email = input.Email;
            user.Role = input.Role;
            user.Active = input.Active;
            user.MustChangePassword = input.MustChangePassword;

            await _db.SaveChangesAsync();
            TempData["ok"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/ToggleActive/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            user.Active = !user.Active;

            if (user.Active)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutUntil = null;
            }

            await _db.SaveChangesAsync();

            TempData["ok"] = user.Active ? "Usuario ACTIVADO." : "Usuario DESACTIVADO.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/ResetPassword/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            var temp = GenerateTempPassword();
            var (hash, salt) = PasswordHelper.Hash(temp);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.MustChangePassword = true;

            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;
            user.LastPasswordChange = null;

            await _db.SaveChangesAsync();

            TempData["ok"] = $"Contraseña temporal: {temp}";
            return RedirectToAction(nameof(Index));
        }

        private static string GenerateTempPassword()
        {
            // Formato: IT-<4 letras>-<4 números>!
            var rnd = Random.Shared;
            var letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            var sb = new StringBuilder();
            sb.Append("IT-");
            for (int i = 0; i < 4; i++) sb.Append(letters[rnd.Next(letters.Length)]);
            sb.Append("-");
            sb.Append(rnd.Next(1000, 9999));
            sb.Append("!");
            return sb.ToString();
        }
    }
}
