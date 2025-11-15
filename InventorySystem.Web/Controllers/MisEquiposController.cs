using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "USER,ADMIN")]
public class MisEquiposController : Controller
{
    public IActionResult Index() => View();
}
