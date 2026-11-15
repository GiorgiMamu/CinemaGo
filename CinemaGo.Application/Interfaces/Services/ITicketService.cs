using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface ITicketService
{
    Task<TicketResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TicketResponse> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<TicketResponse> CheckInAsync(string code, CancellationToken ct = default);
}