using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<BusRoute> Routes { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasDiscriminator(u => u.Role)
                .HasValue<Customer>("User")
                .HasValue<Admin>("Admin")
                .HasValue<Operator>("Operator");

            // Operator -> Bus
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Operator)
                .WithMany(o => o.Buses)
                .HasForeignKey(b => b.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Route -> Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bus -> Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Bus)
                .WithMany(b => b.Trips)
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip -> Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Booking (Passenger)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip -> Expense
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // Operator -> Expense
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Operator)
                .WithMany(o => o.Expenses)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Bus>()
                .HasIndex(b => b.BusNumber)
                .IsUnique();
        }
    }
}
