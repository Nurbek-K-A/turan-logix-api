using System.Security.Claims;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Api.Extensions;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public UserRole? Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            return claim is not null && Enum.TryParse<UserRole>(claim.Value, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
