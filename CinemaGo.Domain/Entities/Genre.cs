using System;
using System.Collections.Generic;
using System.Text;
using CinemaGo.Domain.Common;
namespace CinemaGo.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}