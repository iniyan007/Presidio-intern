using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace DataAccessLayer.Contexts
{
   public class NotificationsContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notification_2app;Username=postgres;Password=iniyanavin");
        }

        public DbSet<Notification>  Notification { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(n =>
            {
                n.HasKey(n => n.Id).HasName("PK_NotificationId");
                n.Property(n => n.Time).HasColumnType("timestamp without time zone");
                n.Property(n => n.Message).HasColumnType("text");
                n.Property(n => n.ToAddress).HasColumnType("varchar");
                n.Property(n => n.UserId).HasColumnType("integer");
                n.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .HasConstraintName("FK_Notification_User")
                .OnDelete(DeleteBehavior.SetNull);
                n.HasDiscriminator<string>("NotificationCategory")
                .HasValue<Notification>("Notification")
                .HasValue<Email>("Email")
                .HasValue<Sms>("Sms");

            });
            modelBuilder.Entity<User>(u =>
            {
            u.HasKey(u => u.Id).HasName("PK_UserId");
            u.Property(u => u.Name).HasColumnType("varchar");
            u.Property(u => u.Email).HasColumnType("varchar");
            u.Property(u => u.Phone).HasColumnType("varchar");
            });
        }
    }
}
