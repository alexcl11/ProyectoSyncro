using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoSyncro.Extensions;
using ProyectoSyncro.Models;
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
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                await this.repo.CreateTablaEmpresaAsync(idEmpresa, nombreTabla);
                return RedirectToAction("Index", "Dashboard", new { tabla = nombreTabla });
            }
            return RedirectToAction("Index", "Dashboard");
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                List<string> tablas = await this.repo.GetTablasEmpresaAsync(idEmpresa);

                ViewData["TablasEmpresa"] = tablas;
                ViewData["NombreUser"] = HttpContext.Session.GetObject<UserSession>("User").Nombre;
                ViewData["NombreEmpresa"] = HttpContext.Session.GetObject<UserSession>("User").NombreEmpresa;
                await next();
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}