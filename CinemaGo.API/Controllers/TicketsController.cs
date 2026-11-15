using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[Authorize]
public class TicketsController : BaseApiController
{
    private readonly ITicketService _service;
    public TicketsController(ITicketService service) => _service = service;

    /// <summary>Get ticket details by id (owner or staff).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Get ticket details by ticket code (owner or staff).</summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> GetByCode(string code, CancellationToken ct)
        => Success(await _service.GetByCodeAsync(code, ct), SuccessMessages.Retrieved);

    /// <summary>Check a ticket in at the door (Admin). Works once, from 60 minutes before the session until it ends; cancelled tickets are rejected.</summary>
    [HttpPost("by-code/{code}/check-in"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> CheckIn(string code, CancellationToken ct)
        => Success(await _service.CheckInAsync(code, ct), SuccessMessages.TicketCheckedIn);
}