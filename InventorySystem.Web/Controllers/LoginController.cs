using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using InventorySystem.Web.Data;
using InventorySystem.Web.Data.Entities;
using InventorySystem.Web.Security;

public class LoginController : Controller
{
    private readonly InventoryContext _db;
    public LoginController(InventoryContext db) => _db = db;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {
        // Permite JER01 (Name) o correo (Email)
        var user = _db.AppUsers.FirstOrDefault(u => (u.Email == username || u.Name == username));
        if (user == null || !user.Active)
        { ModelState.AddModelError("", "Usuario o contraseña inválidos."); return View(); }

        // Bloqueo temporal
        if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
        { ModelState.AddModelError("", $"Cuenta bloqueada hasta {user.LockoutUntil}"); return View(); }

        // Verifica PBKDF2
        if (user.PasswordSalt == null || user.PasswordHash == null ||
            !PasswordHelper.Verify(password, user.PasswordSalt, user.PasswordHash))
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(20);
                user.FailedLoginAttempts = 0;
            }
            _db.SaveChanges();
            ModelState.AddModelError("", "Usuario o contraseña inválidos.");
            return View();
        }

        // Ok → limpia contadores
        user.FailedLoginAttempts = 0;
        user.LockoutUntil = null;
        _db.SaveChanges();

        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name ?? username),
            new Claim(ClaimTypes.Email, user.Email ?? username),
            new Claim(ClaimTypes.Role, user.Role) 
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // Cambio de contraseña SOLO si must_change_password = 1
        if (user.MustChangePassword) return RedirectToAction("ChangePassword", "Account");

        return user.Role == "ADMIN" ? Redirect("/Dashboard") : Redirect("/MisEquipos");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }
}
