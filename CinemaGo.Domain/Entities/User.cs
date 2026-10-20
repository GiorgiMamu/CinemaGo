using System;
using System.Collections.Generic;
using System.Text;
using CinemaGo.Domain.Common;
namespace CinemaGo.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}