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

        public async Task<IActionResult> Index(string tabla, string sortCol = "Id", string sortDir = "DESC")
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                List<string> tablas = await this.repo.GetTablasEmpresaAsync(idEmpresa);
                if (tabla != null && tablas.Contains(tabla))
                {
                    List<Dictionary<string, object>> datos = 
                        await this.repo.GetDatosTablaEmpresaAsync(idEmpresa, tabla, sortCol, sortDir);
                    var columnas = await this.repo.GetColumnasTablaAsync(idEmpresa, tabla);
                    var opciones = await this.repo.GetOpcionesSelectTablaEmpresaAsync(idEmpresa, tabla);
                    var relaciones = await this.repo.GetOpcionesRelacionTablaEmpresaAsync(idEmpresa, tabla);

                    ViewData["NombreUser"] = HttpContext.Session.GetObject<UserSession>("User").Nombre;
                    ViewData["TablasEmpresa"] = tablas;
                    ViewData["Title"] = tabla;
                    ViewData["Columnas"] = columnas;
                    ViewData["OpcionesSelect"] = opciones;
                    ViewData["OpcionesRelacion"] = relaciones;
                    ViewData["SortCol"] = sortCol;
                    ViewData["SortDir"] = sortDir;

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

        [HttpPost]
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

        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> DeleteTablasEmpresa(List<string> nombresTablas)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                try
                {
                    if (nombresTablas != null && nombresTablas.Any())
                    {
                        foreach (string tabla in nombresTablas)
                        {
                            await this.repo.DeleteTablasEmpresaAsync(idEmpresa, tabla);
                        }
                    }
                    return Ok();
                }
                catch (SqlException ex) when (ex.Number == 547 || ex.Number == 3726)
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
        public async Task<IActionResult> DeleteColumna(string nombreTabla, string nombreColumna)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") != null)
            {
                int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
                try
                {
                    await this.repo.DeleteColumnaAsync(idEmpresa, nombreTabla, nombreColumna);
                    return Ok();
                }
                catch (SqlException ex) when (ex.Number == 5074) 
                {
                    return BadRequest("No se puede eliminar esta columna porque está atada a una restricción o relación de la base de datos.");
                }
                catch (Exception ex)
                {
                    return BadRequest("Ocurrió un error al intentar eliminar la columna.");
                }
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenameTabla(string nombreOld, string nombreNew)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") == null) return RedirectToAction("Login", "Auth");

            int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
            try
            {
                await this.repo.RenameTablaAsync(idEmpresa, nombreOld, nombreNew);
                return Ok(new { url = $"/Dashboard/Index?tabla={nombreNew}" });
            }
            catch (Exception ex)
            {
                return BadRequest("No se pudo renombrar la tabla. Es posible que el nombre ya exista.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenameColumna(
    string nombreTabla, string nombreOld, string nombreNew, string tipoDato,
    string? nombreTablaRelacionada, List<string> opcionesValor, List<string> opcionesColor)
        {
            if (HttpContext.Session.GetObject<UserSession>("User") == null) return RedirectToAction("Login", "Auth"); 

            int idEmpresa = HttpContext.Session.GetObject<UserSession>("User").IdEmpresa;
            try
            {
                await this.repo.RenameColumnaAsync(idEmpresa, nombreTabla, nombreOld, nombreNew, tipoDato, nombreTablaRelacionada);

                if (tipoDato == "Select" && opcionesValor != null && opcionesValor.Count > 0)
                {
                    for (int i = 0; i < opcionesValor.Count; i++)
                    {
                        string valor = opcionesValor[i];
                        if (!string.IsNullOrWhiteSpace(valor))
                        {
                            string color = (opcionesColor != null && opcionesColor.Count > i) ? opcionesColor[i] : "#64748b";
                            await this.repo.InsertarOpcionColumnaAsync(idEmpresa, nombreTabla, nombreNew, valor, color);
                        }
                    }
                }

                
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("No se pudo actualizar la columna. Es posible que el tipo de dato no sea compatible con los registros actuales (ej: intentar pasar letras a números).");
            }
        }
    }
}