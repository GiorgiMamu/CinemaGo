using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface ISessionService
{
    Task<List<SessionResponse>> GetAllAsync(CancellationToken ct = default);
    Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default);
    Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
}