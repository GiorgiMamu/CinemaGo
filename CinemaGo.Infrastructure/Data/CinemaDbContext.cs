using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Data;

public class CinemaDbContext : IdentityDbContext<ApplicationUser>
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);   

        b.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.UserName).HasMaxLength(50);
            e.Property(u => u.Email).HasMaxLength(256);
            e.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");
        });
        b.Entity<IdentityRole>().ToTable("Roles");
        b.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        b.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        b.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        b.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        b.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        b.Entity<Genre>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(SeedData.Genres);
        });

        b.Entity<Movie>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.PosterPath).HasMaxLength(500);
            e.HasIndex(x => x.Title).IsUnique();
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Movies_Duration", "[DurationMinutes] > 0");
                t.HasCheckConstraint("CK_Movies_AgeRating", "[AgeRating] >= 0");
            });
            e.HasData(SeedData.Movies);
        });

        b.Entity<MovieGenre>(e =>
        {
            e.HasKey(x => new { x.MovieId, x.GenreId });
            e.HasOne(x => x.Movie).WithMany(m => m.MovieGenres).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Genre).WithMany(g => g.MovieGenres).HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
            e.HasData(SeedData.MovieGenres);
        });

        b.Entity<Hall>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(SeedData.Halls);
        });

        b.Entity<Seat>(e =>
        {
            e.HasOne(x => x.Hall).WithMany(h => h.Seats).HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber }).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Seats_Positive", "[RowNumber] > 0 AND [SeatNumber] > 0"));
            e.HasData(SeedData.Seats);
        });

        b.Entity<Session>(e =>
        {
            e.Property(x => x.StandardPrice).HasPrecision(10, 2);
            e.Property(x => x.VipPrice).HasPrecision(10, 2);
            e.HasOne(x => x.Movie).WithMany(m => m.Sessions).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Hall).WithMany(h => h.Sessions).HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.HallId, x.StartsAt });
            e.ToTable(t => t.HasCheckConstraint("CK_Sessions_Prices", "[StandardPrice] >= 0 AND [VipPrice] >= 0"));
        });

        b.Entity<Booking>(e =>
        {
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Session).WithMany(s => s.Bookings).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Seat).WithMany(s => s.Bookings).HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.SessionId, x.SeatId }).IsUnique()
                .HasFilter($"[Status] = {(int)BookingStatus.Active}");
            e.HasIndex(x => x.UserId);
        });

        b.Entity<Ticket>(e =>
        {
            e.Property(x => x.TicketCode).IsRequired().HasMaxLength(50);
            e.Property(x => x.FinalPrice).HasPrecision(10, 2);
            e.HasIndex(x => x.TicketCode).IsUnique();
            e.HasOne(x => x.Booking).WithOne(x => x.Ticket).HasForeignKey<Ticket>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.BookingId).IsUnique();
            e.Property(x => x.Status).HasDefaultValue(TicketStatus.Valid);
        });
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }
}