using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Account");

            if (User.IsInRole("ADMIN"))
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "MisEquipos");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
