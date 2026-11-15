using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetPagedAsync(UserFilter filter, CancellationToken ct = default);
    Task<UserResponse> CreateAdminAsync(CreateAdminRequest request, CancellationToken ct = default);
    Task<UserResponse> ChangeRoleAsync(string userId, ChangeRoleRequest request, CancellationToken ct = default);
    Task<UserResponse> SetActiveAsync(string userId, SetActiveRequest request, CancellationToken ct = default);
    Task DeleteAsync(string userId, CancellationToken ct = default);
    Task DeleteMyAccountAsync(DeleteAccountRequest request, CancellationToken ct = default);
}