using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ProyectoSyncro.Services; // 👈 Referencia a tus nuevos servicios
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoSyncro.Policies
{
    public class FreeTierRequirement : IAuthorizationRequirement
    {
    }

    public class FreeTierTableLimitHandler : AuthorizationHandler<FreeTierRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FreeTierTableLimitHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FreeTierRequirement requirement)
        {
            // Comprobamos si está autenticado
            if (!context.User.Identity.IsAuthenticated) return;

            // Si el usuario es Premium, le dejamos pasar instantáneamente
            if (context.User.HasClaim("Plan", "Premium"))
            {
                context.Succeed(requirement);
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // 🔄 ¡AQUÍ ESTÁ EL CAMBIO! Pedimos el BaseApiService en lugar del Repository
                var apiService = httpContext.RequestServices.GetService<BaseApiService>();

                if (apiService != null)
                {
                    // Llamamos a nuestra API en Azure para saber cuántas tablas tiene
                    var tablas = await apiService.GetTablasAsync();

                    // Si es plan Free y tiene 0, 1, o 2 tablas, lo dejamos pasar
                    if (tablas.Count < 3)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        // Si ya tiene 3 o más y no es Premium, le bloqueamos la acción en el MVC
                        context.Fail();
                    }
                }
            }
        }
    }
}