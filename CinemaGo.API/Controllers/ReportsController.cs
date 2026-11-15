using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[Authorize(Roles = RoleNames.AdminOrAbove)]
public class ReportsController : BaseApiController
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    /// <summary>Tickets sold and revenue per movie (GroupBy/Count/Sum). Cancelled tickets are excluded.</summary>
    [HttpGet("revenue-by-movie")]
    public async Task<ActionResult<ApiResponse<List<RevenueByMovieResponse>>>> Revenue(DateTime? from, DateTime? to, CancellationToken ct)
        => Success(await _service.GetRevenueByMovieAsync(from, to, ct), SuccessMessages.Retrieved);

    /// <summary>Occupancy of every non-cancelled session: booked seats vs total seats.</summary>
    [HttpGet("session-occupancy")]
    public async Task<ActionResult<ApiResponse<List<SessionOccupancyResponse>>>> Occupancy(DateTime? from, DateTime? to, CancellationToken ct)
        => Success(await _service.GetSessionOccupancyAsync(from, to, ct), SuccessMessages.Retrieved);
}