using System;
using System.Collections.Generic;
using System.Text;
namespace CinemaGo.Domain.Entities;

public class MovieGenre
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}