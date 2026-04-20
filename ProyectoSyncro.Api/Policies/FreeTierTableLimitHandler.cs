using Microsoft.AspNetCore.Authorization;
using ProyectoSyncro.Api.Repositories; // Tu namespace de la API
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoSyncro.Api.Policies
{
    public class FreeTierRequirement : IAuthorizationRequirement
    {
    }

    public class FreeTierTableLimitHandler : AuthorizationHandler<FreeTierRequirement>
    {
        // 👇 Inyectamos el repositorio directamente, sin HttpContextAccessor. ¡Mucho más limpio!
        private readonly BaseRepository _repo;

        public FreeTierTableLimitHandler(BaseRepository repo)
        {
            _repo = repo;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FreeTierRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated) return;

            // Si el usuario es Premium, le dejamos pasar instantáneamente
            if (context.User.HasClaim("Plan", "Premium"))
            {
                context.Succeed(requirement);
                return;
            }

            var claimIdEmpresa = context.User.FindFirst("IdEmpresa");

            if (claimIdEmpresa != null)
            {
                int idEmpresa = int.Parse(claimIdEmpresa.Value);

                var tablas = await _repo.GetTablasEmpresaAsync(idEmpresa);

                // Si es plan Free y tiene 0, 1, o 2 tablas, lo dejamos pasar
                if (tablas.Count < 3)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    // Bloqueo total por backend
                    context.Fail();
                }
            }
        }
    }
}