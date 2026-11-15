using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface ISessionService
{
    Task<PagedResult<SessionResponse>> GetPagedAsync(SessionFilter filter, CancellationToken ct = default);
    Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<SeatAvailabilityResponse>> GetSeatsAsync(int sessionId, CancellationToken ct = default);
    Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default);
    Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
    Task<int> FinishPastSessionsAsync(CancellationToken ct = default);
}