using System;
using System.Collections.Generic;
using BookMart.Model.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace BookMart.DataAccess;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookAuthorMap> BookAuthorMaps { get; set; }

    public virtual DbSet<BookDetail> BookDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(e => e.Isbn, "IX_Books_ISBN")
                .IsUnique()
                .HasFilter("([ISBN] IS NOT NULL)");

            entity.HasIndex(e => e.PublisherId, "IX_Books_PublisherId");

            entity.HasIndex(e => e.TenantId, "IX_Books_TenantId").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CustomCoverColor).HasMaxLength(30);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsLimitedEdition).HasDefaultValue(false);
            entity.Property(e => e.Isbn)
                .HasMaxLength(32)
                .HasColumnName("ISBN");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SearchVector).HasMaxLength(768);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookAuthorMap>(entity =>
        {
            entity.HasKey(e => new { e.BookId, e.AuthorId });

            entity.HasIndex(e => e.AuthorId, "IX_BookAuthorMaps_AuthorId");

            entity.Property(e => e.ContributionRole).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Author).WithMany(p => p.BookAuthorMaps).HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Book).WithMany(p => p.BookAuthorMaps).HasForeignKey(d => d.BookId);
        });

        modelBuilder.Entity<BookDetail>(entity =>
        {
            entity.HasKey(e => e.BookId);

            entity.Property(e => e.BookId).ValueGeneratedNever();
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.Property(e => e.Language).HasMaxLength(50);

            entity.HasOne(d => d.Book).WithOne(p => p.BookDetail).HasForeignKey<BookDetail>(d => d.BookId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.HasIndex(e => e.ProductName, "IX_Products");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Isbn)
                .HasMaxLength(22)
                .IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Postal_Code");
            entity.Property(e => e.Price).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
