using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "ADMIN")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
