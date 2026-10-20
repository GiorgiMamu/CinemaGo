using CinemaGo.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;
namespace CinemaGo.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int AgeRating { get; set; }
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}