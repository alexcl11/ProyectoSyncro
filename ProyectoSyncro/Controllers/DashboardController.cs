using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            List<string> tablas = await this.repo.GetTablasEmpresaAsync(idEmpresa);
            if (tabla != null && tablas.Contains(tabla))
            {
                List<Dictionary<string, object>> datos = 
                    await this.repo.GetDatosTablaEmpresaAsync(idEmpresa, tabla);
                var columnas = await this.repo.GetColumnasTablaAsync(idEmpresa, tabla);
                var opciones = await this.repo.GetOpcionesSelectTablaEmpresaAsync(idEmpresa, tabla);
                var relaciones = await this.repo.GetOpcionesRelacionTablaEmpresaAsync(idEmpresa, tabla);
                

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

        public async Task<IActionResult> CreateRegistro(string nombreTabla, Dictionary<string, string> valoresRegistro)
        {
            int idEmpresa = 1;

            await this.repo.InsertRegistroTablaEmpresaAsync(idEmpresa, nombreTabla, valoresRegistro);

            return RedirectToAction("Index", new { tabla = nombreTabla });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCelda(string nombreTabla, int idFila, string columna, string valor)
        {
            int idEmpresa = 1; 

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


        [HttpPost]
        public async Task<IActionResult> DeleteRegistros(string nombreTabla, List<int> idsFilas)
        {
            int idEmpresa = 1;

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

        [HttpPost]
        public async Task<IActionResult> DeleteRegistro(string nombreTabla, int idFila)
        {
            int idEmpresa = 1;

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
    }
}