using CinemaGo.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;
namespace CinemaGo.Domain.Entities;

public class Hall : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}