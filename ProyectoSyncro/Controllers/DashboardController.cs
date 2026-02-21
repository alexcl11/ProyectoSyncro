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
                var opciones = await this.repo.GetOpcionesSelectTablaEmpresa(idEmpresa, tabla);
                var relaciones = await this.repo.GetOpcionesRelacionTablaEmpresa(idEmpresa, tabla);
                List<string> tablas = await this.repo.GetTablasEmpresaAsync(1);

                ViewData["TablasEmpresa"] = tablas;
                ViewData["Title"] = tabla;
                ViewData["Columnas"] = columnas;
                ViewData["OpcionesSelect"] = opciones;
                ViewData["OpcionesRelacion"] = relaciones;
                
                return View(datos);
            } else
            {
                return View();
            }
                
        }

        public async Task<IActionResult> CreateColumn
            (string nombreTabla, string nombreColumna, string tipoDato, string? nombreTablaRelacionada, 
            List<string> opcionesValor, List<string> opcionesColor)
        {
            int idEmpresa = 1;

            await this.repo.CreateColumnaTabla(idEmpresa, nombreTabla, nombreColumna, tipoDato, nombreTablaRelacionada);

            if (tipoDato == "Select" && opcionesValor != null && opcionesValor.Count > 0)
            {
                for (int i = 0; i < opcionesValor.Count; i++)
                {
                    string valor = opcionesValor[i];

                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        string color = (opcionesColor != null && opcionesColor.Count > i) ? opcionesColor[i] : "#64748b";
                        await this.repo.InsertarOpcionColumnaAsync(idEmpresa, nombreTabla, nombreColumna, valor, color);
                    }
                }
            }
            

            return RedirectToAction("Index", new { tabla = nombreTabla });
        }

        public async Task<IActionResult> CreateRegistro(string nombreTabla, Dictionary<string, string> valoresRegistro)
        {
            int idEmpresa = 1;

            await this.repo.InsertRegistroTablaEmpresaAsync(idEmpresa, nombreTabla, valoresRegistro);

            return RedirectToAction("Index", new { tabla = nombreTabla });
        }
    }
}