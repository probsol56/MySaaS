using Microsoft.AspNetCore.Http;
using MySaaS.Application.Common.Interfaces;
using System.Security.Claims;

namespace MySaaS.Infrastructure.Services
{
    public class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid? TenantId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                    return null;

                // Get TenantId from authenticated user's claims (secure)
                var tenantIdClaim = httpContext.User.FindFirst("TenantId");
                if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    return tenantId;
                }

                return null;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }
    }
}
