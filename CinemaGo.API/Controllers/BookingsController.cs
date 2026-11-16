using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[Authorize]
public class BookingsController : BaseApiController
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) => _service = service;

    /// <summary>Book one or several seats of a session in one transaction. Each booking gets a ticket with a unique code. Admins can book for a customer with onBehalfOfUserId.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAsync(request, ct), SuccessMessages.BookingCreated);


    /// <summary>List all bookings of all customers with filters (user, session, movie, status, booking date) and paging (Admin).</summary>
    [HttpGet, Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingResponse>>>> GetAll([FromQuery] BookingFilter filter, CancellationToken ct)
        => Success(await _service.GetAllAsync(filter, ct), SuccessMessages.Retrieved);


    /// <summary>My booking history, newest first, with paging.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingResponse>>>> My([FromQuery] PageRequest page, CancellationToken ct)
        => Success(await _service.GetMyAsync(page, ct), SuccessMessages.Retrieved);

    /// <summary>Get one booking (owner or staff).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Cancel a booking before the session starts (owner or Admin). It stays with a CancelledAt time, its ticket becomes Cancelled, and the customer is e-mailed when staff cancel it.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Cancel(int id, CancellationToken ct)
    { await _service.CancelAsync(id, ct); return Success(SuccessMessages.BookingCancelled); }
}