namespace CinemaGo.Domain.DTOs;

public class MovieRequest
{
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int AgeRating { get; set; }
    public List<int> GenreIds { get; set; } = new();
}