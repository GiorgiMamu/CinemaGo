using AutoMapper;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Application.Profiles;

public class CinemaMappingProfile : Profile
{
    public CinemaMappingProfile()
    {
        // Genre
        CreateMap<Genre, GenreResponse>();
        CreateMap<GenreRequest, Genre>();

        // Movie (genres are linked manually in MovieService)
        CreateMap<Movie, MovieResponse>()
            .ForMember(d => d.PosterUrl, o => o.MapFrom(s => s.PosterPath))
            .ForMember(d => d.Genres, o => o.MapFrom(s => s.MovieGenres.Select(mg => mg.Genre.Name).OrderBy(n => n)));
        CreateMap<MovieRequest, Movie>()
            .ForMember(d => d.MovieGenres, o => o.Ignore())
            .ForMember(d => d.Sessions, o => o.Ignore())
            .ForMember(d => d.PosterPath, o => o.Ignore());

        // Hall
        CreateMap<Hall, HallResponse>();
        CreateMap<HallRequest, Hall>();

        // Seat
        CreateMap<Seat, SeatResponse>();
        CreateMap<SeatRequest, Seat>()
            .ForMember(d => d.Hall, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore());

        // Session (needs Movie + Hall loaded)
        CreateMap<Session, SessionResponse>()
            .ForMember(d => d.PosterUrl, o => o.MapFrom(s => s.Movie.PosterPath))
            .ForMember(d => d.EndsAt, o => o.MapFrom(s => s.StartsAt.AddMinutes(s.Movie.DurationMinutes)));
        CreateMap<SessionRequest, Session>()
            .ForMember(d => d.Movie, o => o.Ignore())
            .ForMember(d => d.Hall, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());

        // Booking (needs Session.Movie, Session.Hall, Seat, Ticket loaded)
        CreateMap<Ticket, TicketSummaryResponse>();
        CreateMap<Booking, BookingResponse>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Session.Movie.Title))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Session.Hall.Name))
            .ForMember(d => d.StartsAt, o => o.MapFrom(s => s.Session.StartsAt))
            .ForMember(d => d.RowNumber, o => o.MapFrom(s => s.Seat.RowNumber))
            .ForMember(d => d.SeatNumber, o => o.MapFrom(s => s.Seat.SeatNumber))
            .ForMember(d => d.IsVip, o => o.MapFrom(s => s.Seat.IsVip));

        // Ticket details
        CreateMap<Ticket, TicketResponse>()
            .ForMember(d => d.BookingStatus, o => o.MapFrom(s => s.Booking.Status))
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Booking.Session.Movie.Title))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Booking.Session.Hall.Name))
            .ForMember(d => d.StartsAt, o => o.MapFrom(s => s.Booking.Session.StartsAt))
            .ForMember(d => d.RowNumber, o => o.MapFrom(s => s.Booking.Seat.RowNumber))
            .ForMember(d => d.SeatNumber, o => o.MapFrom(s => s.Booking.Seat.SeatNumber))
            .ForMember(d => d.IsVip, o => o.MapFrom(s => s.Booking.Seat.IsVip));
    }
}