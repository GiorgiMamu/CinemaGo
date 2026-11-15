namespace CinemaGo.Domain.DTOs;

public class MovieResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int AgeRating { get; set; }
    public string? PosterUrl { get; set; }
    public List<string> Genres { get; set; } = new();
}