using System.Security.Claims;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Constants;
namespace CinemaGo.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
    public bool IsStaff => IsInRole(RoleNames.Admin) || IsInRole(RoleNames.SuperAdmin);
}