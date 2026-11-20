using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Encapsulates the Fluent API configuration for the Author entity.
    /// Demonstrates how to enforce schema constraints and properly handle computed C# properties.
    /// </summary>
    internal class AuthorEntityConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            #region 1. Table & Primary Key Mapping

            builder.ToTable($"{nameof(Author)}s");

            // Fluent API way to declare the Primary key. 
            // Alternatively, you can use the [Key] Data Annotation attribute on the entity class.
            builder.HasKey(nameof(Author.Id));

            #endregion

            #region 2. Column Configurations & Constraints

            // Reference NOTE: String Constraints
            // In SQL Server, an unconfigured C# string maps to NVARCHAR(MAX). 
            // Setting explicit MaxLengths here is critical for database performance, indexing efficiency, 
            // and preventing malicious users from inserting massive payloads into standard text fields.

            builder.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(150);

            // If using the enriched Author model from our seed data, you would map those here:
            // builder.Property(a => a.Expertise).HasMaxLength(100);
            // builder.Property(a => a.Biography).HasMaxLength(2000);
            // builder.Property(a => a.WebsiteUrl).HasMaxLength(255);

            #endregion

            #region 3. Ignored Properties (Not Mapped to Database)

            // ==========================================
            // DEMO NOTE: COMPUTED & IGNORED PROPERTIES
            // ==========================================
            // Ignoring computed or UI-only properties.
            // This prevents EF Core from trying to create a 'Name' column in the database
            // since it is just a C# getter that concatenates FirstName and LastName.
            // 
            // Talking Point: You already have the [NotMapped] data annotation on your entity doing this! 
            // This shows the Fluent API equivalent. As a team standard, it is best to choose one approach 
            // (Data Annotations vs Fluent API) to avoid confusion, with Fluent API being the preferred 
            // choice for keeping Domain Models completely clean of database-specific logic.

            builder.Ignore(x => x.Name);

            #endregion
        }
    }
}