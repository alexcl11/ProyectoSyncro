using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoSyncro.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ProyectoSyncro.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _authService;

        public AuthController(AuthApiService authService)
        {
            _authService = authService;
        }

        public IActionResult Register(string plan = "free")
        {
            ViewData["PLANSELECCIONADO"] = plan;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string plan, string nombreEmpresa, string cif, string nombreAdmin, string email, string password)
        {
            // Pedimos a la API que registre al usuario
            bool registrado = await _authService.RegisterAsync(cif, nombreEmpresa, nombreAdmin, email, password);

            if (registrado)
            {
                //hacemos login automático
                string token = await _authService.LoginAsync(email, password);

                if (token != null)
                {
                    HttpContext.Session.SetString("TOKEN", token);

                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(8) });

                    if (plan == "premium")
                    {
                        return RedirectToAction("Checkout", "Payment");
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ViewData["MENSAJE"] = "Hubo un error al registrar la empresa. Es posible que el CIF o Email ya existan.";
            return View();
        }

        public IActionResult Login(string? mensaje)
        {
            ViewData["MENSAJE"] = mensaje;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Llamamos a nuestra API para conseguir el Token JWT
            string token = await _authService.LoginAsync(email, password);

            if (!string.IsNullOrEmpty(token))
            {
                // Guardamos el token en la sesión para que los demás servicios puedan usarlo
                HttpContext.Session.SetString("TOKEN", token);

                // extraemos los Claims del JWT para crear la Identidad en el MVC
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                });

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                TempData["InvalidCredentials"] = "Credenciales incorrectas";
            }
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Limpiamos el Token
            return RedirectToAction("Login", "Auth");
        }
    }
}