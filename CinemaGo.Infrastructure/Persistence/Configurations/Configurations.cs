using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CinemaGo.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.Name).IsUnique();
        b.HasData(new Role { Id = 1, Name = "Customer" }, new Role { Id = 2, Name = "Admin" });
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.UserName).IsRequired().HasMaxLength(50);
        b.Property(x => x.Email).IsRequired().HasMaxLength(256);
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.UserName).IsUnique();
        b.HasOne(x => x.Role).WithMany(r => r.Users)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> b)
    {
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Movies_Duration", "[DurationMinutes] > 0");
            t.HasCheckConstraint("CK_Movies_AgeRating", "[AgeRating] >= 0");
        });
    }
}

public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> b)
    {
        b.HasKey(x => new { x.MovieId, x.GenreId });
        b.HasOne(x => x.Movie).WithMany(m => m.MovieGenres)
            .HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Genre).WithMany(g => g.MovieGenres)
            .HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> b)
    {
        b.HasOne(x => x.Hall).WithMany(h => h.Seats)
            .HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber }).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_Seats_Positive", "[RowNumber] > 0 AND [SeatNumber] > 0"));
    }
}

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.Property(x => x.StandardPrice).HasPrecision(10, 2);
        b.Property(x => x.VipPrice).HasPrecision(10, 2);
        b.HasOne(x => x.Movie).WithMany(m => m.Sessions)
            .HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Hall).WithMany(h => h.Sessions)
            .HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.HallId, x.StartsAt });
        b.ToTable(t => t.HasCheckConstraint("CK_Sessions_Prices", "[StandardPrice] >= 0 AND [VipPrice] >= 0"));
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.HasOne(x => x.User).WithMany(u => u.Bookings)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Session).WithMany(s => s.Bookings)
            .HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Seat).WithMany(s => s.Bookings)
            .HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);

        // one seat per session may have only ONE active booking (Status = 1)
        b.HasIndex(x => new { x.SessionId, x.SeatId })
            .IsUnique()
            .HasFilter($"[Status] = {(int)BookingStatus.Active}");
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.Property(x => x.TicketCode).IsRequired().HasMaxLength(50);
        b.Property(x => x.FinalPrice).HasPrecision(10, 2);
        b.HasIndex(x => x.TicketCode).IsUnique();
        b.HasOne(x => x.Booking).WithOne(x => x.Ticket)
            .HasForeignKey<Ticket>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.BookingId).IsUnique();   // enforces one-to-one in the database
    }
}