using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Security;
using System.Security.Claims;

[Authorize]
public class AccountController : Controller
{
    private readonly InventoryContext _db;
    public AccountController(InventoryContext db) => _db = db;

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.AppUsers.First(u => u.UserId == uid);
        if (!user.MustChangePassword)
            return Redirect(user.Role == "ADMIN" ? "/Dashboard" : "/MisEquipos");
        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.AppUsers.First(u => u.UserId == uid);

        if (!user.MustChangePassword)
            return Redirect(user.Role == "ADMIN" ? "/Dashboard" : "/MisEquipos");

        if (user.PasswordSalt == null || user.PasswordHash == null ||
            !PasswordHelper.Verify(currentPassword, user.PasswordSalt, user.PasswordHash))
        { ModelState.AddModelError("", "Contraseña actual incorrecta."); return View(); }

        if (newPassword != confirmPassword) { ModelState.AddModelError("", "Las contraseñas no coinciden."); return View(); }
        if (!PasswordPolicy.IsStrong(newPassword, out var err)) { ModelState.AddModelError("", err); return View(); }

        var (hash, salt) = PasswordHelper.Hash(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.MustChangePassword = false;
        user.LastPasswordChange = DateTime.UtcNow;
        _db.SaveChanges();

        TempData["ok"] = "Contraseña actualizada.";
        return Redirect(user.Role == "ADMIN" ? "/Dashboard" : "/MisEquipos");
    }
}
