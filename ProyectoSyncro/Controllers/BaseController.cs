using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoSyncro.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSyncro.Controllers
{
    public class BaseController : Controller
    {
        private readonly BaseRepository repo; 

        public BaseController(BaseRepository repo)
        {
            this.repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string nombreTabla)
        {
            await this.repo.CreateTablaEmpresaAsync(1, nombreTabla);
            return RedirectToAction("Index", "Dashboard", new { tabla = nombreTabla });
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            List<string> tablas = await this.repo.GetTablasEmpresaAsync(1);

            ViewData["TablasEmpresa"] = tablas;

            await next();
        }
    }
}