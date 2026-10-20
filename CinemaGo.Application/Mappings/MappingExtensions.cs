using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Application.Mappings;

public static class MappingExtensions
{
    public static GenreResponse ToResponse(this Genre g) => new GenreResponse
    {
        Id = g.Id,
        Name = g.Name
    };

    // requires MovieGenres + Genre to be loaded (Include/ThenInclude)
    public static MovieResponse ToResponse(this Movie m) => new MovieResponse
    {
        Id = m.Id,
        Title = m.Title,
        DurationMinutes = m.DurationMinutes,
        AgeRating = m.AgeRating,
        Genres = m.MovieGenres.Select(x => x.Genre.Name).OrderBy(n => n).ToList()
    };

    public static HallResponse ToResponse(this Hall h) => new HallResponse
    {
        Id = h.Id,
        Name = h.Name
    };

    public static SeatResponse ToResponse(this Seat s) => new SeatResponse
    {
        Id = s.Id,
        HallId = s.HallId,
        RowNumber = s.RowNumber,
        SeatNumber = s.SeatNumber,
        IsVip = s.IsVip
    };

    // requires Movie and Hall to be loaded
    public static SessionResponse ToResponse(this Session s) => new SessionResponse
    {
        Id = s.Id,
        MovieId = s.MovieId,
        MovieTitle = s.Movie.Title,
        HallId = s.HallId,
        HallName = s.Hall.Name,
        StartsAt = s.StartsAt,
        StandardPrice = s.StandardPrice,
        VipPrice = s.VipPrice,
        Status = s.Status
    };

    public static RoleResponse ToResponse(this Role r) => new RoleResponse
    {
        Id = r.Id,
        Name = r.Name
    };

    // requires Role to be loaded
    public static UserResponse ToResponse(this User u) => new UserResponse
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Role = u.Role.Name
    };

    public static BookingResponse ToResponse(this Booking b) => new BookingResponse
    {
        Id = b.Id,
        UserId = b.UserId,
        SessionId = b.SessionId,
        SeatId = b.SeatId,
        Status = b.Status,
        CreatedAt = b.CreatedAt
    };

    public static TicketResponse ToResponse(this Ticket t) => new TicketResponse
    {
        Id = t.Id,
        BookingId = t.BookingId,
        TicketCode = t.TicketCode,
        FinalPrice = t.FinalPrice
    };
}