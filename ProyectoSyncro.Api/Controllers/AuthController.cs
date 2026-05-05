using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoSyncro.Api.Repositories;
using ProyectoSyncro.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProyectoSyncro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly SecretClient _secretClient;

        public AuthController(AuthRepository repo, IConfiguration config, SecretClient secretClient)
        {
            _repo = repo;
            _config = config;
            _secretClient = secretClient;
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validamos credenciales contra SQL Server usando tu repo intacto
            UserSession user = await _repo.LoginUserAsync(request.Email, request.Password);

            if (user == null)
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });
            }

            // 2. Si es correcto, fabricamos sus "Claims" (lo que va dentro del token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim("Email", user.Email),
                new Claim("IdUsuario", user.IdUsuario.ToString()),
                new Claim("IdEmpresa", user.IdEmpresa.ToString()),
                new Claim("NombreEmpresa", user.NombreEmpresa ?? "Mi Empresa"),
                new Claim("Plan", user.IsPremium ? "Premium" : "Free"),
                new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
            };

            // 3. Firmamos el token con nuestra clave secreta
            KeyVaultSecret jwtKeySecret = await _secretClient.GetSecretAsync("jwt-secretkey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeySecret.Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4), // El token dura 4 horas
                signingCredentials: creds
            );

            // 4. Devolvemos el token en formato string
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
        }

        public class RegisterRequest
        {
            public string Cif { get; set; }
            public string NombreEmpresa { get; set; }
            public string NombreUsuario { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Llama a tu procedimiento almacenado que encripta la clave y crea todo
                int result = await _repo.RegisterEmpresaUserAsync(
                    request.Cif,
                    request.NombreEmpresa,
                    request.NombreUsuario,
                    request.Email,
                    request.Password);

                // Asumimos que si devuelve un número mayor a 0, fue un éxito (ajusta según tu SP)
                if (result > 0)
                {
                    return Ok(new { mensaje = "Empresa y usuario registrados con éxito" });
                }
                return BadRequest(new { mensaje = "El usuario o CIF ya existe" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor: " + ex.Message });
            }
        }

    }
}