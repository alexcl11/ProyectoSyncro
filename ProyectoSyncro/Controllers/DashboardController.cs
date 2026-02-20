using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Repositories;

namespace ProyectoSyncro.Controllers
{
    public class DashboardController : BaseController
    {
        private BaseRepository repo;
        public DashboardController(BaseRepository repo) : base(repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index(string tabla)
        {
            if (tabla != null)
            {
                List<Dictionary<string, object>> datos = 
                    await this.repo.GetDatosTablaEmpresaAsync(1, tabla);
                ViewData["Title"] = tabla;
                return View(datos);
            } else
            {
                return View();
            }
                
        }
    }
}