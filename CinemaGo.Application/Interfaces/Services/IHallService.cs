using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IHallService
{
    Task<List<HallResponse>> GetAllAsync(CancellationToken ct = default);
    Task<HallResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HallResponse> CreateAsync(HallRequest request, CancellationToken ct = default);
    Task<HallResponse> UpdateAsync(int id, HallRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}