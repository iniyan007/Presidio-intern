using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Context;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }


    private static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return string.Concat(System.Linq.Enumerable.Select(text, (x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingTraveler> BookingTravelers { get; set; }

    public virtual DbSet<ItineraryActivity> ItineraryActivities { get; set; }

    public virtual DbSet<ItineraryDay> ItineraryDays { get; set; }

    public virtual DbSet<ItineraryDayMeal> ItineraryDayMeals { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageThread> MessageThreads { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackageAccommodation> PackageAccommodations { get; set; }

    public virtual DbSet<PackageHighlight> PackageHighlights { get; set; }

    public virtual DbSet<PackageInclusion> PackageInclusions { get; set; }

    public virtual DbSet<PackageMedium> PackageMedia { get; set; }

    public virtual DbSet<PackageSeasonalPricing> PackageSeasonalPricings { get; set; }

    public virtual DbSet<PackageTransport> PackageTransports { get; set; }

    public virtual DbSet<Packager> Packagers { get; set; }

    public virtual DbSet<PackagerDocument> PackagerDocuments { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlatformConfig> PlatformConfigs { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewMedium> ReviewMedia { get; set; }

    public virtual DbSet<TravelDocument> TravelDocuments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    public virtual DbSet<VBookingRevenue> VBookingRevenues { get; set; }

    public virtual DbSet<VPackagerRevenueSummary> VPackagerRevenueSummaries { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("booking_status", new[] { "pending", "confirmed", "cancelled", "completed", "refunded" })
            .HasPostgresEnum("document_status", new[] { "uploaded", "verified", "rejected" })
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.MessageSenderRole>("message_sender_role")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.NotificationType>("notification_type")
            .HasPostgresEnum("packager_status", new[] { "pending", "approved", "suspended", "deactivated" })
            .HasPostgresEnum("payment_status", new[] { "unpaid", "partial", "paid", "refunded", "failed" })
            .HasPostgresEnum("review_status", new[] { "pending", "published", "flagged", "removed" })
            .HasPostgresEnum("user_role", new[] { "user", "admin", "packager" })
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.DaySession>("day_session")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.InclusionType>("inclusion_type")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.MealType>("meal_type")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.MediaCategory>("media_category")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.PackageStatus>("package_status")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.PackageType>("package_type")
            .HasPostgresEnum<TravelTourManagement.DataAccess.Enums.TransportMode>("transport_mode")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => e.Action, "idx_audit_action");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_audit_entity");

            entity.HasIndex(e => e.PerformedAt, "idx_audit_performed_at").IsDescending();

            entity.HasIndex(e => e.PerformedBy, "idx_audit_performed_by");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValues)
                .HasColumnType("jsonb")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasColumnType("jsonb")
                .HasColumnName("old_values");
            entity.Property(e => e.PerformedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("performed_at");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.PerformedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("audit_logs_performed_by_fkey");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.HasIndex(e => e.BookingReference, "bookings_booking_reference_key").IsUnique();

            entity.HasIndex(e => e.PackageId, "idx_bookings_package_id");

            entity.HasIndex(e => e.BookingReference, "idx_bookings_reference");

            entity.HasIndex(e => e.TravelDate, "idx_bookings_travel_date");

            entity.HasIndex(e => e.UserId, "idx_bookings_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AdultCount)
                .HasDefaultValue(1)
                .HasColumnName("adult_count");
            entity.Property(e => e.BookedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("booked_at");
            entity.Property(e => e.BookingReference)
                .HasMaxLength(30)
                .HasColumnName("booking_reference");
            entity.Property(e => e.CancellationReason).HasColumnName("cancellation_reason");
            entity.Property(e => e.CancelledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.ChildCount)
                .HasDefaultValue(0)
                .HasColumnName("child_count");
            entity.Property(e => e.InfantCount)
                .HasDefaultValue(0)
                .HasColumnName("infant_count");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackagerBaseAmount)
                .HasPrecision(12, 2)
                .HasColumnName("packager_base_amount");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(12, 2)
                .HasColumnName("paid_amount");
            entity.Property(e => e.PlatformFeeAmount)
                .HasPrecision(12, 2)
                .HasColumnName("platform_fee_amount");
            entity.Property(e => e.PlatformFeePercent)
                .HasPrecision(5, 2)
                .HasColumnName("platform_fee_percent");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.SeasonalPricingId).HasColumnName("seasonal_pricing_id");
            entity.Property(e => e.SpecialRequests).HasColumnName("special_requests");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.Status)
                .HasColumnName("status");
                
            entity.Property(e => e.PaymentStatus)
                .HasColumnName("payment_status");

            entity.HasOne(d => d.Package).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bookings_package_id_fkey");

            entity.HasOne(d => d.SeasonalPricing).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.SeasonalPricingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bookings_seasonal_pricing_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("bookings_user_id_fkey");
        });

        modelBuilder.Entity<BookingTraveler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("booking_travelers_pkey");

            entity.ToTable("booking_travelers");

            entity.HasIndex(e => e.BookingId, "idx_travelers_booking_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
                .HasColumnName("nationality");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(e => e.MealPreference).HasColumnName("meal_preference").HasMaxLength(50);
            entity.Property(e => e.AadharCardNumber).HasColumnName("aadhar_card_number").HasMaxLength(50);
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(50)
                .HasColumnName("passport_number");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingTravelers)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("booking_travelers_booking_id_fkey");
        });

        modelBuilder.Entity<ItineraryActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("itinerary_activities_pkey");

            entity.ToTable("itinerary_activities");

            entity.HasIndex(e => e.ItineraryDayId, "idx_activities_day_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ActivityTitle)
                .HasMaxLength(200)
                .HasColumnName("activity_title");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(100)
                .HasColumnName("activity_type");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.ExtraCost)
                .HasPrecision(10, 2)
                .HasColumnName("extra_cost");
            entity.Property(e => e.IsOptional)
                .HasDefaultValue(false)
                .HasColumnName("is_optional");
            entity.Property(e => e.ItineraryDayId).HasColumnName("itinerary_day_id");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.SequenceOrder)
                .HasDefaultValue(1)
                .HasColumnName("sequence_order");
            entity.Property(e => e.DaySession).HasColumnName("session").ValueGeneratedNever();

            entity.HasOne(d => d.ItineraryDay).WithMany(p => p.ItineraryActivities)
                .HasForeignKey(d => d.ItineraryDayId)
                .HasConstraintName("itinerary_activities_itinerary_day_id_fkey");
        });

        modelBuilder.Entity<ItineraryDay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("itinerary_days_pkey");

            entity.ToTable("itinerary_days");

            entity.HasIndex(e => e.PackageId, "idx_itinerary_days_package_id");

            entity.HasIndex(e => new { e.PackageId, e.DayNumber }, "uq_itinerary_day").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DayNumber).HasColumnName("day_number");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Package).WithMany(p => p.ItineraryDays)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("itinerary_days_package_id_fkey");
        });

        modelBuilder.Entity<ItineraryDayMeal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("itinerary_day_meals_pkey");

            entity.ToTable("itinerary_day_meals");

            entity.HasIndex(e => e.ItineraryDayId, "idx_meals_day_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.IsIncluded)
                .HasDefaultValue(true)
                .HasColumnName("is_included");
            entity.Property(e => e.ItineraryDayId).HasColumnName("itinerary_day_id");
            entity.Property(e => e.Venue)
                .HasMaxLength(200)
                .HasColumnName("venue");
            entity.Property(e => e.MealType).HasColumnName("meal").ValueGeneratedNever();

            entity.HasOne(d => d.ItineraryDay).WithMany(p => p.ItineraryDayMeals)
                .HasForeignKey(d => d.ItineraryDayId)
                .HasConstraintName("itinerary_day_meals_itinerary_day_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.HasIndex(e => e.SenderId, "idx_messages_sender_id");

            entity.HasIndex(e => e.ThreadId, "idx_messages_thread_id");

            entity.HasIndex(e => new { e.ThreadId, e.IsRead }, "idx_messages_unread").HasFilter("(is_read = false)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderRole)
                .HasColumnName("sender_role");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.ThreadId).HasColumnName("thread_id");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("messages_sender_id_fkey");

            entity.HasOne(d => d.Thread).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ThreadId)
                .HasConstraintName("messages_thread_id_fkey");
        });

        modelBuilder.Entity<MessageThread>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("message_threads_pkey");

            entity.ToTable("message_threads");

            entity.HasIndex(e => e.PackageId, "idx_threads_package_id");

            entity.HasIndex(e => e.PackagerId, "idx_threads_packager_id");

            entity.HasIndex(e => e.UserId, "idx_threads_user_id");

            entity.HasIndex(e => new { e.UserId, e.PackagerId, e.PackageId }, "uq_thread").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LastMessageAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_message_at");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Package).WithMany(p => p.MessageThreads)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("message_threads_package_id_fkey");

            entity.HasOne(d => d.Packager).WithMany(p => p.MessageThreads)
                .HasForeignKey(d => d.PackagerId)
                .HasConstraintName("message_threads_packager_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.MessageThreads)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("message_threads_user_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_notifications_unread").HasFilter("(is_read = false)");

            entity.HasIndex(e => e.UserId, "idx_notifications_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_user_id_fkey");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("packages_pkey");

            entity.ToTable("packages");

            entity.HasIndex(e => e.Country, "idx_packages_country");

            entity.HasIndex(e => e.Destination, "idx_packages_dest_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Destination, "idx_packages_destination");

            entity.HasIndex(e => e.IsFeatured, "idx_packages_featured").HasFilter("(is_featured = true)");

            entity.HasIndex(e => e.PackagerId, "idx_packages_packager_id");

            entity.HasIndex(e => e.AvgRating, "idx_packages_rating").IsDescending();

            entity.HasIndex(e => e.Title, "idx_packages_title_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AvgRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("avg_rating");
            entity.Property(e => e.CancellationPolicy).HasColumnName("cancellation_policy");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentBookings)
                .HasDefaultValue(0)
                .HasColumnName("current_bookings");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Destination)
                .HasMaxLength(200)
                .HasColumnName("destination");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.DurationNights)
                .HasDefaultValue(0)
                .HasColumnName("duration_nights");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false)
                .HasColumnName("is_featured");
            entity.Property(e => e.MaxCapacity).HasColumnName("max_capacity");
            entity.Property(e => e.MinAge).HasColumnName("min_age");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");
            entity.Property(e => e.TotalReviews)
                .HasDefaultValue(0)
                .HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Type).HasColumnName("package_type").ValueGeneratedNever();
            entity.Property(e => e.Status).HasColumnName("status").ValueGeneratedNever();

            entity.HasOne(d => d.Packager).WithMany(p => p.Packages)
                .HasForeignKey(d => d.PackagerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("packages_packager_id_fkey");
        });

        modelBuilder.Entity<PackageAccommodation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_accommodation_pkey");

            entity.ToTable("package_accommodation");

            entity.HasIndex(e => e.ItineraryDayId, "idx_accommodation_day_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Amenities).HasColumnName("amenities");
            entity.Property(e => e.CheckInTime).HasColumnName("check_in_time");
            entity.Property(e => e.CheckOutTime).HasColumnName("check_out_time");
            entity.Property(e => e.HotelAddress)
                .HasMaxLength(400)
                .HasColumnName("hotel_address");
            entity.Property(e => e.HotelName)
                .HasMaxLength(200)
                .HasColumnName("hotel_name");
            entity.Property(e => e.ItineraryDayId).HasColumnName("itinerary_day_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RoomType)
                .HasMaxLength(100)
                .HasColumnName("room_type");
            entity.Property(e => e.StarRating).HasColumnName("star_rating");

            entity.HasOne(d => d.ItineraryDay).WithMany(p => p.PackageAccommodations)
                .HasForeignKey(d => d.ItineraryDayId)
                .HasConstraintName("package_accommodation_itinerary_day_id_fkey");
        });

        modelBuilder.Entity<PackageHighlight>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_highlights_pkey");

            entity.ToTable("package_highlights");

            entity.HasIndex(e => e.PackageId, "idx_highlights_package_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.HighlightText)
                .HasMaxLength(200)
                .HasColumnName("highlight_text");
            entity.Property(e => e.PackageId).HasColumnName("package_id");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageHighlights)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("package_highlights_package_id_fkey");
        });

        modelBuilder.Entity<PackageInclusion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_inclusions_pkey");

            entity.ToTable("package_inclusions");

            entity.HasIndex(e => e.PackageId, "idx_inclusions_package_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Type).HasColumnName("type").ValueGeneratedNever();

            entity.HasOne(d => d.Package).WithMany(p => p.PackageInclusions)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("package_inclusions_package_id_fkey");
        });

        modelBuilder.Entity<PackageMedium>(entity =>
        {
            entity.Property(e => e.Category).HasColumnName("category").ValueGeneratedNever();
            entity.HasKey(e => e.Id).HasName("package_media_pkey");

            entity.ToTable("package_media");

            entity.HasIndex(e => e.PackageId, "idx_media_package_id");

            entity.HasIndex(e => new { e.PackageId, e.IsPrimary }, "idx_media_primary").HasFilter("(is_primary = true)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Caption)
                .HasMaxLength(300)
                .HasColumnName("caption");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageMedia)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("package_media_package_id_fkey");
        });

        modelBuilder.Entity<PackageSeasonalPricing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_seasonal_pricing_pkey");

            entity.ToTable("package_seasonal_pricing");

            entity.HasIndex(e => e.IsActive, "idx_pricing_active").HasFilter("(is_active = true)");

            entity.HasIndex(e => new { e.StartDate, e.EndDate }, "idx_pricing_dates");

            entity.HasIndex(e => e.PackageId, "idx_pricing_package_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AvailableSlots).HasColumnName("available_slots");
            entity.Property(e => e.BasePrice)
                .HasPrecision(12, 2)
                .HasColumnName("base_price");
            entity.Property(e => e.ChildPrice)
                .HasPrecision(12, 2)
                .HasColumnName("child_price");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasColumnName("discount_percent");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.SeasonName)
                .HasMaxLength(100)
                .HasColumnName("season_name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageSeasonalPricings)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("package_seasonal_pricing_package_id_fkey");
        });

        modelBuilder.Entity<PackageTransport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_transport_pkey");

            entity.ToTable("package_transport");

            entity.HasIndex(e => e.ItineraryDayId, "idx_transport_day_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.DistanceKm)
                .HasPrecision(8, 2)
                .HasColumnName("distance_km");
            entity.Property(e => e.DropPoint)
                .HasMaxLength(300)
                .HasColumnName("drop_point");
            entity.Property(e => e.DropTime).HasColumnName("drop_time");
            entity.Property(e => e.ItineraryDayId).HasColumnName("itinerary_day_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PickupPoint)
                .HasMaxLength(300)
                .HasColumnName("pickup_point");
            entity.Property(e => e.PickupTime).HasColumnName("pickup_time");
            entity.Property(e => e.SegmentOrder)
                .HasDefaultValue(1)
                .HasColumnName("segment_order");
            entity.Property(e => e.VehicleDescription)
                .HasMaxLength(200)
                .HasColumnName("vehicle_description");
            entity.Property(e => e.TransportMode).HasColumnName("mode").ValueGeneratedNever();

            entity.HasOne(d => d.ItineraryDay).WithMany(p => p.PackageTransports)
                .HasForeignKey(d => d.ItineraryDayId)
                .HasConstraintName("package_transport_itinerary_day_id_fkey");
        });

        modelBuilder.Entity<Packager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("packagers_pkey");

            entity.ToTable("packagers");

            entity.HasIndex(e => e.AvgRating, "idx_packagers_rating").IsDescending();

            entity.HasIndex(e => e.UserId, "idx_packagers_user_id");

            entity.HasIndex(e => e.UserId, "packagers_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.AvgRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("avg_rating");
            entity.Property(e => e.BusinessLicenseNo)
                .HasMaxLength(100)
                .HasColumnName("business_license_no");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .HasColumnName("company_name");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(20)
                .HasColumnName("contact_phone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeactivatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deactivated_at");
            entity.Property(e => e.DeactivationReason).HasColumnName("deactivation_reason");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.TotalReviews)
                .HasDefaultValue(0)
                .HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(500)
                .HasColumnName("website_url");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.PackagerApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("packagers_approved_by_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.PackagerUser)
                .HasForeignKey<Packager>(d => d.UserId)
                .HasConstraintName("packagers_user_id_fkey");
        });

        modelBuilder.Entity<PackagerDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("packager_documents_pkey");

            entity.ToTable("packager_documents");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .HasColumnName("document_type");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasColumnName("file_path");
            entity.Property(e => e.OriginalFilename)
                .HasMaxLength(255)
                .HasColumnName("original_filename");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Packager).WithMany(p => p.PackagerDocuments)
                .HasForeignKey(d => d.PackagerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("packager_documents_packager_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.BookingId, "idx_payments_booking_id");

            entity.HasIndex(e => e.PaidAt, "idx_payments_paid_at");

            entity.HasIndex(e => e.TransactionId, "payments_transaction_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValueSql("'INR'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.GatewayResponse).HasColumnName("gateway_response");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(100)
                .HasColumnName("payment_method");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refunded_at");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(200)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("payments_booking_id_fkey");
        });

        modelBuilder.Entity<PlatformConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("platform_config_pkey");

            entity.ToTable("platform_config");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.GstPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("gst_percent");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PlatformFeePercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("5.00")
                .HasColumnName("platform_fee_percent");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PlatformConfigs)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("platform_config_updated_by_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.OverallRating, "idx_reviews_overall").IsDescending();

            entity.HasIndex(e => e.PackageId, "idx_reviews_package_id");

            entity.HasIndex(e => e.PackagerId, "idx_reviews_packager_id");

            entity.HasIndex(e => e.UserId, "idx_reviews_user_id");

            entity.HasIndex(e => e.BookingId, "reviews_booking_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AccommodationRating).HasColumnName("accommodation_rating");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FoodRating).HasColumnName("food_rating");
            entity.Property(e => e.GuideRating).HasColumnName("guide_rating");
            entity.Property(e => e.IsVerifiedTraveler)
                .HasDefaultValue(true)
                .HasColumnName("is_verified_traveler");
            entity.Property(e => e.OverallRating).HasColumnName("overall_rating");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.TransportRating).HasColumnName("transport_rating");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ValueRating).HasColumnName("value_rating");

            entity.HasOne(d => d.Booking).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reviews_booking_id_fkey");

            entity.HasOne(d => d.Package).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reviews_package_id_fkey");

            entity.HasOne(d => d.Packager).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PackagerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reviews_packager_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reviews_user_id_fkey");
        });

        modelBuilder.Entity<ReviewMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("review_media_pkey");

            entity.ToTable("review_media");

            entity.HasIndex(e => e.ReviewId, "idx_review_media_review_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewMedia)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("review_media_review_id_fkey");
        });

        modelBuilder.Entity<TravelDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("travel_documents_pkey");

            entity.ToTable("travel_documents");

            entity.HasIndex(e => e.BookingId, "idx_docs_booking_id");

            entity.HasIndex(e => e.TravelerId, "idx_docs_traveler_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(100)
                .HasColumnName("document_type");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.OriginalFilename)
                .HasMaxLength(255)
                .HasColumnName("original_filename");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.TravelerId).HasColumnName("traveler_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.VerifiedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.Booking).WithMany(p => p.TravelDocuments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("travel_documents_booking_id_fkey");

            entity.HasOne(d => d.Traveler).WithMany(p => p.TravelDocuments)
                .HasForeignKey(d => d.TravelerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("travel_documents_traveler_id_fkey");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.TravelDocuments)
                .HasForeignKey(d => d.VerifiedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("travel_documents_verified_by_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.IsActive, "idx_users_active");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_email_verified");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(500)
                .HasColumnName("profile_picture");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<VBookingRevenue>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_booking_revenue");

            entity.Property(e => e.AdultCount).HasColumnName("adult_count");
            entity.Property(e => e.BookedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("booked_at");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BookingReference)
                .HasMaxLength(30)
                .HasColumnName("booking_reference");
            entity.Property(e => e.ChildCount).HasColumnName("child_count");
            entity.Property(e => e.Destination)
                .HasMaxLength(200)
                .HasColumnName("destination");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageTitle)
                .HasMaxLength(300)
                .HasColumnName("package_title");
            entity.Property(e => e.PackagerBaseAmount)
                .HasPrecision(12, 2)
                .HasColumnName("packager_base_amount");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.PackagerName)
                .HasMaxLength(200)
                .HasColumnName("packager_name");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(12, 2)
                .HasColumnName("paid_amount");
            entity.Property(e => e.PlatformFeeAmount)
                .HasPrecision(12, 2)
                .HasColumnName("platform_fee_amount");
            entity.Property(e => e.PlatformFeePercent)
                .HasPrecision(5, 2)
                .HasColumnName("platform_fee_percent");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("user_email");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(150)
                .HasColumnName("user_name");
            entity.Property(e => e.UserPhone)
                .HasMaxLength(20)
                .HasColumnName("user_phone");
        });

        modelBuilder.Entity<VPackagerRevenueSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_packager_revenue_summary");

            entity.Property(e => e.CancelledBookings).HasColumnName("cancelled_bookings");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .HasColumnName("company_name");
            entity.Property(e => e.CompletedBookings).HasColumnName("completed_bookings");
            entity.Property(e => e.ConfirmedBookings).HasColumnName("confirmed_bookings");
            entity.Property(e => e.PackagerId).HasColumnName("packager_id");
            entity.Property(e => e.TotalBookings).HasColumnName("total_bookings");
            entity.Property(e => e.TotalEarned).HasColumnName("total_earned");
            entity.Property(e => e.TotalGmv).HasColumnName("total_gmv");
            entity.Property(e => e.TotalPlatformFee).HasColumnName("total_platform_fee");
        });
        modelBuilder.HasSequence("booking_ref_seq");

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wishlists_pkey");
            entity.ToTable("wishlists");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("wishlists_user_id_fkey");

            entity.HasOne(d => d.Package).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("wishlists_package_id_fkey");

            // Ensure unique constraint so a user can't favorite the same package twice
            entity.HasIndex(e => new { e.UserId, e.PackageId }).IsUnique().HasDatabaseName("ix_wishlists_user_id_package_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
