using Microsoft.EntityFrameworkCore;
using BusBooking.Domain.Entities;

namespace BusBooking.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<Bus> Buses { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<SeatLock> SeatLocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "USER" },
            new Role { Id = 2, Name = "OPERATOR" },
            new Role { Id = 3, Name = "ADMIN" }
        );

        modelBuilder.Entity<SeatLock>()
            .HasIndex(s => new { s.TripId, s.SeatNumber })
            .IsUnique();
    }
}