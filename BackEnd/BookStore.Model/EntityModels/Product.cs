using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Model.EntityModels
{
    /// <summary>
    /// Represents a generic Product entity.
    /// Heavily relies on Data Annotations to explicitly configure the database schema, 
    /// overriding EF Core's default conventions directly within the model.
    /// 
    /// Reference Note: The [Table] attribute explicitly maps this class to a specific table name in the database. 
    ///  This overrides the default EF convention which typically uses the DbSet property name (e.g., "Products").
    /// </summary>
    [Table("Product")]
    [Index(nameof(ProductName), IsUnique = false, AllDescending = false, Name = "IX_Products")]
    public class Product
    {
        #region Core Identifiers

        // Reference Note: Identifies the property as the Primary Key. 
        // For integer properties named 'Id', EF Core infers this automatically, but being explicit is a good practice.
        [Key]
        public int Id { get; set; }

        #endregion

        #region Standard Data Properties

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        // The 'required' keyword ensures C# compile-time safety against null instantiation.

        public required string ProductName { get; set; }

        public string? City { get; set; }
        public string Region { get; set; }

        #endregion

        #region Explicit Column & Type Mapping

        // Reference Note: The [Column] attribute allows mapping a C# property to a specific SQL column name that breaks standard naming conventions (e.g., snake_case "Postal_Code"). It also forces the specific SQL data type.
        [Column("Postal_Code", TypeName = "varchar(200)")]
        public required string PostalCode { get; set; }

        // Reference Note: For decimal types, it is highly recommended to specify the precision and scale explicitly to prevent truncation or default to decimal(18,2).
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Price { get; set; }

        // Reference Note: By default, EF Core maps C# strings to SQL Server's NVARCHAR (Unicode). [Unicode(false)] forces it to VARCHAR instead. This cuts the storage space in half, which is perfect for fields like ISBNs that will never contain special international characters.
        [Unicode(false)]
        [MaxLength(22)]
        public string Isbn { get; set; }

        #endregion

        #region Database Generated Properties

        // Reference Note: Informs EF Core that the database engine will generate a value for this property when a new row is inserted (e.g., via a SQL DEFAULT constraint). EF Core will not attempt to send a C# value for this property in the INSERT statement, but will retrieve the generated value back.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedOn { get; set; }

        #endregion

        #region Computed & Ignored Properties

        // Reference Note: [NotMapped] explicitly prevents Entity Framework Core from attempting to create a 'Fullname' column in the physical database. It exists purely in memory.
        [NotMapped]
        public string Fullname => $"{FirstName} {LastName}";

        #endregion
    }
}