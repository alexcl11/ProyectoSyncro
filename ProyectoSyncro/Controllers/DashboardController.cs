using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Repositories;

namespace ProyectoSyncro.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(BaseRepository repo) : base(repo)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}