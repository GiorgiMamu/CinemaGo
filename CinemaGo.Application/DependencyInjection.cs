using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Application.Profiles;
using CinemaGo.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace CinemaGo.Application;
using CinemaGo.Application.Validators;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        services.AddAutoMapper(cfg =>
        {
            // optional free Community License key; without it AutoMapper works and only logs a warning
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey)) cfg.LicenseKey = autoMapperLicenseKey;
        }, typeof(CinemaMappingProfile).Assembly);

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}