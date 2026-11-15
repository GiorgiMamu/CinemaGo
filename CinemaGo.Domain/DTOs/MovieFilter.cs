namespace CinemaGo.Domain.DTOs;

public class MovieFilter : PageRequest
{
    public string? Search { get; set; }
    public int? GenreId { get; set; }
    public int? MaxAgeRating { get; set; }
    public string? SortBy { get; set; }      // title (default) | duration | rating
    public bool Descending { get; set; }
}