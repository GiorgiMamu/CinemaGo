using System;
using System.Collections.Generic;
using System.Text;
using CinemaGo.Domain.Common;
namespace CinemaGo.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // Customer / Admin
    public ICollection<User> Users { get; set; } = new List<User>();
}