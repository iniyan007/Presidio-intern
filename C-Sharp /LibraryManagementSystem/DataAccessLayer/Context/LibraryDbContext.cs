using System;
using System.Collections.Generic;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<Borrow> Borrows { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<FinePayment> FinePayments { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MembershipType> MembershipTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_pkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_category_id_fkey");
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("book_copies_pkey");

            entity.Property(e => e.Status).HasDefaultValue(0);

            entity.HasOne(d => d.Book).WithMany(p => p.BookCopies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("book_copies_book_id_fkey");
        });

        modelBuilder.Entity<Borrow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("borrow_pkey");

            entity.Property(e => e.DateOfBorrow).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.Status).HasDefaultValue(0);

            entity.HasOne(d => d.BookCopy).WithMany(p => p.Borrows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("borrow_book_copy_id_fkey");

            entity.HasOne(d => d.Member).WithMany(p => p.Borrows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("borrow_member_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");
        });

        modelBuilder.Entity<FinePayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fine_payments_pkey");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Borrow).WithMany(p => p.FinePayments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fine_payments_borrow_id_fkey");

            entity.HasOne(d => d.Member).WithMany(p => p.FinePayments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fine_payments_member_id_fkey");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("members_pkey");

            entity.Property(e => e.JoinedDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.Status).HasDefaultValue(0);

            entity.HasOne(d => d.MembershipType).WithMany(p => p.Members)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("members_membership_type_id_fkey");
        });

        modelBuilder.Entity<MembershipType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("membership_type_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
