using Microsoft.AspNetCore.Mvc;

namespace ProyectoSyncro.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}