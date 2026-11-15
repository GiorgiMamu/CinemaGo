using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Infrastructure.Data;

// Static reference data, inserted by the migration (HasData). IDs are fixed on purpose.
public static class SeedData
{
    
    public static readonly Genre[] Genres =
    {
        new() { Id = 1, Name = "Action" },   new() { Id = 2, Name = "Comedy" },
        new() { Id = 3, Name = "Drama" },    new() { Id = 4, Name = "Horror" },
        new() { Id = 5, Name = "Sci-Fi" },   new() { Id = 6, Name = "Animation" },
        new() { Id = 7, Name = "Thriller" }, new() { Id = 8, Name = "Romance" }
    };

    public static readonly Hall[] Halls =
    {
        new() { Id = 1, Name = "Hall 1" },
        new() { Id = 2, Name = "Hall 2" },
        new() { Id = 3, Name = "VIP Lounge" }
    };

    public static readonly Movie[] Movies =
    {
        new() { Id = 1, Title = "The Last Orbit",      DurationMinutes = 128, AgeRating = 12, Description = "A crew races to repair a failing station before the next orbit." },
        new() { Id = 2, Title = "Midnight in Tbilisi", DurationMinutes = 112, AgeRating = 16, Description = "Two strangers cross paths during one long summer night." },
        new() { Id = 3, Title = "Laugh Factory",       DurationMinutes = 98,  AgeRating = 6,  Description = "A failing comedy club tries one last absurd show." },
        new() { Id = 4, Title = "Shadow Hollow",       DurationMinutes = 104, AgeRating = 18, Description = "A village hides a secret that returns every winter." },
        new() { Id = 5, Title = "Iron Pursuit",        DurationMinutes = 135, AgeRating = 16, Description = "A detective chases a thief across three countries." },
        new() { Id = 6, Title = "Little Dragon",       DurationMinutes = 92,  AgeRating = 0,  Description = "A small dragon learns that courage comes in all sizes." }
    };

    public static readonly MovieGenre[] MovieGenres =
    {
        new() { MovieId = 1, GenreId = 5 }, new() { MovieId = 1, GenreId = 1 },
        new() { MovieId = 2, GenreId = 3 }, new() { MovieId = 2, GenreId = 8 },
        new() { MovieId = 3, GenreId = 2 },
        new() { MovieId = 4, GenreId = 4 }, new() { MovieId = 4, GenreId = 7 },
        new() { MovieId = 5, GenreId = 1 }, new() { MovieId = 5, GenreId = 7 },
        new() { MovieId = 6, GenreId = 6 }, new() { MovieId = 6, GenreId = 2 }
    };

    // Hall 1: 6 rows x 10 (rows 5-6 VIP) | Hall 2: 5 rows x 8 (row 5 VIP) | VIP Lounge: 4 rows x 6 (all VIP)
    public static readonly Seat[] Seats = BuildSeats();

    private static Seat[] BuildSeats()
    {
        var layouts = new (int HallId, int Rows, int PerRow, int VipFromRow)[]
        {
            (1, 6, 10, 5),
            (2, 5, 8, 5),
            (3, 4, 6, 1)
        };

        var seats = new List<Seat>();
        var id = 1;
        foreach (var (hallId, rows, perRow, vipFromRow) in layouts)
            for (var row = 1; row <= rows; row++)
                for (var number = 1; number <= perRow; number++)
                    seats.Add(new Seat
                    {
                        Id = id++,
                        HallId = hallId,
                        RowNumber = row,
                        SeatNumber = number,
                        IsVip = row >= vipFromRow
                    });
        return seats.ToArray();
    }
}