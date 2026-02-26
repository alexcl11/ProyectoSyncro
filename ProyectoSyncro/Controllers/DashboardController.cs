using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoSyncro.Extensions;
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
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                List<string> tablas = await this.repo.GetTablasEmpresaAsync(idEmpresa);
                if (tabla != null && tablas.Contains(tabla))
                {
                    List<Dictionary<string, object>> datos = 
                        await this.repo.GetDatosTablaEmpresaAsync(idEmpresa, tabla);
                    var columnas = await this.repo.GetColumnasTablaAsync(idEmpresa, tabla);
                    var opciones = await this.repo.GetOpcionesSelectTablaEmpresaAsync(idEmpresa, tabla);
                    var relaciones = await this.repo.GetOpcionesRelacionTablaEmpresaAsync(idEmpresa, tabla);

                    ViewData["NombreUser"] = HttpContext.Session.GetObject<UserSession>("User").Nombre;
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
            else
            {
                return RedirectToAction("Login", "Auth");
            }
                
        }

        public async Task<IActionResult> CreateColumn
            (string nombreTabla, string nombreColumna, string tipoDato, string? nombreTablaRelacionada, 
            List<string> opcionesValor, List<string> opcionesColor)
        {

            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                await this.repo.CreateColumnaTablaAsync(idEmpresa, nombreTabla, nombreColumna, tipoDato, nombreTablaRelacionada);

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
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        public async Task<IActionResult> CreateRegistro(string nombreTabla, Dictionary<string, string> valoresRegistro)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;

                await this.repo.InsertRegistroTablaEmpresaAsync(idEmpresa, nombreTabla, valoresRegistro);

                return RedirectToAction("Index", new { tabla = nombreTabla });
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCelda(string nombreTabla, int idFila, string columna, string valor)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                try
                {
                    await this.repo.UpdateCeldaAsync(idEmpresa, nombreTabla, idFila, columna, valor);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message); 
                }
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteRegistros(string nombreTabla, List<int> idsFilas)
        {

            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                try
                {
                    if (idsFilas != null && idsFilas.Any())
                    {
                        await this.repo.DeleteRegistrosAsync(idEmpresa, nombreTabla, idsFilas);
                    }

                    return Ok();
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    return BadRequest("No se puede eliminar porque algunos de estos registros están siendo usados en otras tablas.");
                }
                catch (Exception ex)
                {
                    return BadRequest("Ocurrió un error al intentar eliminar los datos.");
                }
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegistro(string nombreTabla, int idFila)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                try
                {
                    await this.repo.DeleteRegistroAsync(idEmpresa, nombreTabla, idFila);
                    return Ok();
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    return BadRequest("No se puede eliminar porque algunos de estos registros están siendo usados en otras tablas.");
                }
                catch (Exception ex)
                {
                    return BadRequest("Ocurrió un error al intentar eliminar los datos.");
                }
            }
            else
            {
                return RedirectToAction("Login", "Auth");

            }
        }
    }
}