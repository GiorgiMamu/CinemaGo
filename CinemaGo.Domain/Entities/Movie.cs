using CinemaGo.Domain.Common;
namespace CinemaGo.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int AgeRating { get; set; }
    public string? PosterPath { get; set; }   // relative URL, e.g. /uploads/movies/abc.jpg
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}