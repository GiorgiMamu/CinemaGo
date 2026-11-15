using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Ticket?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default);
    Task<List<RevenueByMovieResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<Ticket?> GetByCodeTrackedAsync(string code, CancellationToken ct = default);

}