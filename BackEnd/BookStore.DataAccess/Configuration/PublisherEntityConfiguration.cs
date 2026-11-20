using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Encapsulates the Fluent API configuration for the Publisher entity.
    /// Isolates database schema rules, constraints, and relationships away from the pure C# domain model.
    /// </summary>
    internal class PublisherEntityConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            #region 1. Table & Primary Key Mapping

            // DEMO NOTE: Using string interpolation with nameof() ensures that if we ever 
            // refactor/rename the Publisher class, the table name mapping won't break silently.
            builder.ToTable($"{nameof(Publisher)}s");

            // Fluent API way to declare the Primary key. 
            // Alternatively, you can use the [Key] Data Annotation attribute on the entity property itself.
            builder.HasKey(nameof(Publisher.Id));

            #endregion

            #region 2. Column Configurations & Constraints

            // DEMO NOTE: By default, EF Core maps C# strings to NVARCHAR(MAX) in SQL Server. 
            // Setting a strict MaxLength here optimizes the database storage and indexing capabilities.
            // You can add other Publisher specific column configurations here as needed.
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            #endregion

            #region 3. Relationships (Navigation Properties)

            // DEMO NOTE: Configuring a One-to-Many relationship from the Principal ("One") side.
            // Note: We also configured this exact same relationship from the Dependent (Book) side 
            // inside BookEntityConfiguration. 
            // 
            // Talking Point: EF Core's Model Builder is smart enough to merge these redundant definitions 
            // without throwing an error, but as a best practice, you usually only need to configure the 
            // relationship on ONE side (typically the dependent side where the Foreign Key lives).
            builder.HasMany(p => p.Books)          // Publisher has many Books
                .WithOne(b => b.Publisher)         // Each Book has one Publisher
                .HasForeignKey(b => b.PublisherId) // Explicitly map the Foreign Key property
                .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior (Cascade deletes all books if Publisher is deleted)

            #endregion
        }
    }
}