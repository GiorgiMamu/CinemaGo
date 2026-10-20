using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IMovieService
{
    Task<List<MovieResponse>> GetAllAsync(CancellationToken ct = default);
    Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MovieResponse> CreateAsync(MovieRequest request, CancellationToken ct = default);
    Task<MovieResponse> UpdateAsync(int id, MovieRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}