using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface ISeatService
{
    Task<List<SeatResponse>> GetByHallAsync(int hallId, CancellationToken ct = default);
    Task<SeatResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SeatResponse> CreateAsync(SeatRequest request, CancellationToken ct = default);
    Task<SeatResponse> UpdateAsync(int id, SeatRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}