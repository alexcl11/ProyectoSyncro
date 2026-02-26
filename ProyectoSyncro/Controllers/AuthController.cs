using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using ProyectoSyncro.Extensions;
using ProyectoSyncro.Models;
using ProyectoSyncro.Repositories;
using System.Threading.Tasks;

namespace ProyectoSyncro.Controllers
{
    public class AuthController : Controller
    {
        private AuthRepository repo;
        public AuthController(AuthRepository repo)
        {
            this.repo = repo;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombreEmpresa, string cif, string nombreUsuario, string email, string password)
        {
            int result = await this.repo.RegisterEmpresaUserAsync(cif, nombreEmpresa, nombreUsuario, email, password);

            if (result == 1)
            {
                TempData["Success"] = "Empresa creada correctamente";
                //return RedirectToAction("Login", "Auth");
            }
            else if (result == -1)
            {
                TempData["Error"] = "La empresa ya existe";
            }
            else if (result == -2)
            {
                TempData["Error"] = "El usuario ya existe";
            }
            else
            {
                TempData["Error"] = "Error inesperado, vuelva a probar más tarde";
            }

            return View();
        }

        public IActionResult Login()
        {            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            UserSession usuario = await this.repo.LoginUserAsync(email, password);
            if (usuario != null)
            {
                HttpContext.Session.SetObject("User", usuario);
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {                
                TempData["InvalidCredentials"] = "Credenciales incorrectas";                
            }
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}