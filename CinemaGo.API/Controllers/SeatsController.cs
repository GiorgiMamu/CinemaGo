using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[Authorize(Roles = RoleNames.AdminOrAbove)]
public class SeatsController : BaseApiController
{
    private readonly ISeatService _service;
    public SeatsController(ISeatService service) => _service = service;

    /// <summary>Get one seat.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SeatResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Add a seat to a hall (row and number must be unique in the hall).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SeatResponse>>> Create([FromBody] SeatRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAsync(request, ct), SuccessMessages.Created("Seat"));

    /// <summary>Change a seat (position, VIP flag, hall).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<SeatResponse>>> Update(int id, [FromBody] SeatRequest request, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, request, ct), SuccessMessages.Updated("Seat"));

    /// <summary>Delete a seat. Fails with 409 if it has bookings.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    { await _service.DeleteAsync(id, ct); return Success(SuccessMessages.Deleted("Seat")); }
}