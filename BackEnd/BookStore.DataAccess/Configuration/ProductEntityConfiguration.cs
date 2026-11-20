using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Fluent API configuration for the Product entity.
    /// Replaces all inline database annotations with centralized schema mappings.
    /// </summary>
    internal class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            #region 1. Table, Primary Key & Index Mapping

            // Replaces: [Table("Product")]
            builder.ToTable("Product");

            // Replaces: [Key]
            builder.HasKey(p => p.Id);

            // Replaces: [Index(nameof(ProductName), IsUnique = false, AllDescending = false, Name = "IX_Products")]
            // Note: IsDescending() is available in EF Core 7.0 and later.
            builder.HasIndex(p => p.ProductName)
                .HasDatabaseName("IX_Products")
                .IsUnique(false)
                .IsDescending(false);

            #endregion

            #region 2. Standard Data Properties

            // Replaces: [Required], [MaxLength(50)] on FirstName
            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            // Replaces: [Required], [MaxLength(50)] on LastName
            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Note: The C# 'required' keyword automatically forces EF Core to treat 
            // ProductName as NOT NULL in the schema, but you can explicitly track it here as well.
            builder.Property(p => p.ProductName)
                .IsRequired();

            // Nullable and standard convention strings require no special mapping unless limits are needed.
            builder.Property(p => p.City)
                .IsRequired(false);

            builder.Property(p => p.Region);

            #endregion

            #region 3. Explicit Column & Type Mapping

            // Replaces: [Column("Postal_Code", TypeName = "varchar(200)")]
            builder.Property(p => p.PostalCode)
                .HasColumnName("Postal_Code")
                .HasColumnType("varchar(200)");

            // Replaces: [Column(TypeName = "decimal(5, 2)")]
            builder.Property(p => p.Price)
                .HasColumnType("decimal(5, 2)");

            // Replaces: [Unicode(false)], [MaxLength(22)]
            builder.Property(p => p.Isbn)
                .IsUnicode(false)
                .HasMaxLength(22);

            #endregion

            #region 4. Database Generated Properties

            // Replaces: [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            builder.Property(p => p.CreatedOn)
                .ValueGeneratedOnAdd();

            #endregion

            #region 5. Computed & Ignored Properties

            // Replaces: [NotMapped]
            // This prevents EF Core from trying to look for or create a 'Fullname' column in the physical database table.
            builder.Ignore(p => p.Fullname);

            #endregion
        }
    }
}