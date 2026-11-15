using AutoMapper;
using CinemaGo.Application.Profiles;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
namespace CinemaGo.Tests;

public static class TestData
{
    public static IMapper Mapper()
        => new MapperConfiguration(c => c.AddProfile<CinemaMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    public static Session NewSession(int id = 1, int hallId = 1, DateTime? startsAt = null,
        SessionStatus status = SessionStatus.Scheduled, int duration = 120) => new()
        {
            Id = id,
            HallId = hallId,
            MovieId = 1,
            StartsAt = startsAt ?? DateTime.UtcNow.AddDays(1),
            StandardPrice = 10m,
            VipPrice = 25m,
            Status = status,
            Movie = new Movie { Id = 1, Title = "Test Movie", DurationMinutes = duration },
            Hall = new Hall { Id = hallId, Name = "Hall " + hallId }
        };

    public static Seat NewSeat(int id, int hallId = 1, bool vip = false)
        => new() { Id = id, HallId = hallId, RowNumber = 1, SeatNumber = id, IsVip = vip };
}