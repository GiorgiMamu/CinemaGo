using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync(CancellationToken ct = default);
    Task<RoleResponse> GetByIdAsync(int id, CancellationToken ct = default);
}