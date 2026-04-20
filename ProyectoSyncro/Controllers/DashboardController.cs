using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Services;

namespace ProyectoSyncro.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(BaseApiService baseService) : base(baseService)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string tabla,
            string sortCol = "Id", string sortDir = "DESC",
            string filterCol = null, string filterOp = null, string filterVal = null)
        {
            string nombreUser = HttpContext.User.Identity.Name;
            string plan = HttpContext.User.FindFirst("Plan")?.Value;

            ViewData["esPremium"] = plan != "Free";

            List<string> tablas = await _baseService.GetTablasAsync();

            if (tabla != null && tablas.Contains(tabla))
            {
                var datos = await _baseService.GetDatosTablaAsync(tabla, sortCol, sortDir, filterCol, filterOp, filterVal);
                var columnas = await _baseService.GetColumnasTablaAsync(tabla);
                var opciones = await _baseService.GetOpcionesSelectAsync(tabla);
                var relaciones = await _baseService.GetOpcionesRelacionAsync(tabla);

                ViewData["NombreUser"] = nombreUser;
                ViewData["TablasEmpresa"] = tablas;
                ViewData["Title"] = tabla;
                ViewData["Columnas"] = columnas;
                ViewData["OpcionesSelect"] = opciones;
                ViewData["OpcionesRelacion"] = relaciones;

                ViewData["SortCol"] = sortCol;
                ViewData["SortDir"] = sortDir;
                ViewData["FilterCol"] = filterCol;
                ViewData["FilterOp"] = filterOp;
                ViewData["FilterVal"] = filterVal;

                return View(datos);
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateColumn(string nombreTabla, string nombreColumna, string tipoDato, string? nombreTablaRelacionada, List<string> opcionesValor, List<string> opcionesColor)
        {
            await _baseService.CreateColumnaAsync(nombreTabla, nombreColumna, tipoDato, nombreTablaRelacionada);

            if (tipoDato == "Select" && opcionesValor != null && opcionesValor.Count > 0)
            {
                for (int i = 0; i < opcionesValor.Count; i++)
                {
                    string valor = opcionesValor[i];
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        string color = (opcionesColor != null && opcionesColor.Count > i) ? opcionesColor[i] : "#64748b";
                        await _baseService.InsertarOpcionColumnaAsync(nombreTabla, nombreColumna, valor, color);
                    }
                }
            }
            return RedirectToAction("Index", new { tabla = nombreTabla });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistro(string nombreTabla, Dictionary<string, string> valoresRegistro)
        {
            await _baseService.InsertRegistroAsync(nombreTabla, valoresRegistro);
            return RedirectToAction("Index", new { tabla = nombreTabla });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCelda(string nombreTabla, int idFila, string columna, string valor)
        {
            await _baseService.UpdateCeldaAsync(nombreTabla, idFila, columna, valor);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegistros(string nombreTabla, List<int> idsFilas)
        {
            if (idsFilas != null && idsFilas.Any())
                await _baseService.DeleteRegistrosMultipleAsync(nombreTabla, idsFilas);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegistro(string nombreTabla, int idFila)
        {
            await _baseService.DeleteRegistroAsync(nombreTabla, idFila);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteTablasEmpresa(List<string> nombresTablas)
        {
            if (nombresTablas != null && nombresTablas.Any())
            {
                foreach (string tabla in nombresTablas)
                {
                    await _baseService.DeleteTablaAsync(tabla);
                }
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteColumna(string nombreTabla, string nombreColumna)
        {
            await _baseService.DeleteColumnaAsync(nombreTabla, nombreColumna);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RenameTabla(string nombreOld, string nombreNew)
        {
            await _baseService.RenameTablaAsync(nombreOld, nombreNew);
            return Ok(new { url = $"/Dashboard/Index?tabla={nombreNew}" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RenameColumna(string nombreTabla, string nombreOld, string nombreNew, string tipoDato, string? nombreTablaRelacionada, List<string> opcionesValor, List<string> opcionesColor)
        {
            await _baseService.RenameColumnaAsync(nombreTabla, nombreOld, nombreNew, tipoDato, nombreTablaRelacionada);

            if (tipoDato == "Select" && opcionesValor != null && opcionesValor.Count > 0)
            {
                for (int i = 0; i < opcionesValor.Count; i++)
                {
                    string valor = opcionesValor[i];
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        string color = (opcionesColor != null && opcionesColor.Count > i) ? opcionesColor[i] : "#64748b";
                        await _baseService.InsertarOpcionColumnaAsync(nombreTabla, nombreNew, valor, color);
                    }
                }
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSoloTablas()
        {
            List<string> tablas = await _baseService.GetTablasAsync();
            return Json(tablas);
        }
    }
}