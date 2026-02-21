using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Models;
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
            int idEmpresa = 1;
            if (tabla != null)
            {
                List<Dictionary<string, object>> datos = 
                    await this.repo.GetDatosTablaEmpresaAsync(idEmpresa, tabla);
                var columnas = await this.repo.GetColumnasTablaAsync(idEmpresa, tabla);

                ViewData["Title"] = tabla;
                ViewData["Columnas"] = columnas;
                return View(datos);
            } else
            {
                return View();
            }
                
        }

        public async Task<IActionResult> CreateRegistro(string nombreTabla, Dictionary<string, string> valoresRegistro)
        {
            int idEmpresa = 1;

            await this.repo.InsertRegistroTablaEmpresaAsync(idEmpresa, nombreTabla, valoresRegistro);

            return RedirectToAction("Index", new { tabla = nombreTabla });
        }
    }
}