using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Application.Services;
using Microsoft.Extensions.DependencyInjection;
namespace CinemaGo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}