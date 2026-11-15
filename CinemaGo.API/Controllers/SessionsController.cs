using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

public class SessionsController : BaseApiController
{
    private readonly ISessionService _service;
    public SessionsController(ISessionService service) => _service = service;

    /// <summary>Browse sessions filtered by movie, hall, date range or upcoming only, with paging.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<SessionResponse>>>> GetAll([FromQuery] SessionFilter filter, CancellationToken ct)
        => Success(await _service.GetPagedAsync(filter, ct), SuccessMessages.Retrieved);

    /// <summary>Get one session with movie, hall, prices and end time.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Seat map of a session: every seat with its price and whether it is already booked.</summary>
    [HttpGet("{id:int}/seats"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<SeatAvailabilityResponse>>>> GetSeats(int id, CancellationToken ct)
        => Success(await _service.GetSeatsAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Create a session (Admin). Rejected with 409 if it overlaps another session in the same hall.</summary>
    [HttpPost, Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Create([FromBody] SessionRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAsync(request, ct), SuccessMessages.Created("Session"));

    /// <summary>Change a session's movie, hall, time or prices (Admin).</summary>
    [HttpPut("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<SessionResponse>>> Update(int id, [FromBody] SessionRequest request, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, request, ct), SuccessMessages.Updated("Session"));

    /// <summary>Cancel a session (Admin). It stays in the database as Cancelled; all bookings and tickets are cancelled and customers e-mailed.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse>> Cancel(int id, CancellationToken ct)
    { await _service.CancelAsync(id, ct); return Success(SuccessMessages.SessionCancelled); }
}