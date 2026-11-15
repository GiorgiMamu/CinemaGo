using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Interfaces.Repositories;
namespace CinemaGo.Application.Services;

public class ReportService : IReportService
{
    private readonly ITicketRepository _tickets;
    private readonly ISessionRepository _sessions;
    public ReportService(ITicketRepository tickets, ISessionRepository sessions) { _tickets = tickets; _sessions = sessions; }

    public Task<List<RevenueByMovieResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
        => _tickets.GetRevenueByMovieAsync(from, to, ct);

    public Task<List<SessionOccupancyResponse>> GetSessionOccupancyAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
        => _sessions.GetOccupancyAsync(from, to, ct);
}