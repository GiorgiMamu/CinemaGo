using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IReportService
{
    Task<List<RevenueByMovieResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<List<SessionOccupancyResponse>> GetSessionOccupancyAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}