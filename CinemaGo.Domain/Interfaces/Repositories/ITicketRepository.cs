using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default);
    Task<Ticket?> GetByCodeAsync(string code, CancellationToken ct = default);
}