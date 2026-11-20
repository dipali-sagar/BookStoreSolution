using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Model.EntityModels
{
    /// <summary>
    /// This is the 'Dependent' side of a One-to-One relationship.
    /// It contains extra data that we want to keep separate from the main 'Book' table 
    /// for better database normalization or performance (Table Splitting).
    /// </summary>
    public class BookDetails
    {
        #region Primary & Foreign Key

        // Reference Note: In a strict 1:1 relationship, the Primary Key of the dependent table
        // is also the Foreign Key pointing back to the Principal table.
        // Combining [Key] and [ForeignKey] guarantees at the database schema level that 
        // a Book can never accidentally have more than one associated BookDetails record.
        [Key]
        [ForeignKey("Book")]
        public int BookId { get; set; }

        #endregion

        #region Extended Book Metadata

        // Reference Note: These properties represent extended, specialized metadata.
        // Offloading these into a dependent table (Table Splitting) prevents the primary Book 
        // table from becoming overly wide, improving read performance when this specific 
        // payload is not immediately required.

        public int NumberOfPages { get; set; }

        public int NumberOfChapters { get; set; }

        [MaxLength(50)]
        public string? Format { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        public int? ItemWeightInGrams { get; set; }

        #endregion

        #region Navigation Properties

        // Reference Note: The [DeleteBehavior] attribute is a newer Entity Framework Core 
        // feature that allows defining cascading database rules directly via Data Annotations, 
        // bypassing the need for Fluent API configurations. Cascade ensures that if the parent 
        // Book is deleted, this orphaned detail record is automatically cleaned up by the database.

        // One-to-One relationship back to the Book
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual Book Book { get; set; } = null!;

        #endregion
    }
}