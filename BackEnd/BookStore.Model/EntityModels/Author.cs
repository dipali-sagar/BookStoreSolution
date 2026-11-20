using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookStore.Model.EntityModels
{
    /// <summary>
    /// Represents an Author within the domain.
    /// Demonstrates the use of C# 'required' modifiers for non-nullable properties, 
    /// computed properties excluded from the database schema, and navigation configurations.
    /// </summary>
    public class Author
    {
        #region Core Data Properties

        // Primary Key automatically inferred by EF Core conventions.
        public int Id { get; set; }

        // Reference Note: The 'required' modifier ensures that the object cannot be instantiated 
        // without providing these values. This guarantees strict compile-time safety for 
        // non-nullable database columns and integrates seamlessly with EF Core schema generation.
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Expertise { get; set; }
        public required string Biography { get; set; }
        public required string WebsiteUrl { get; set; }

        #endregion

        #region Computed & Ignored Properties

        // Reference Note: [NotMapped] explicitly prevents Entity Framework Core from attempting 
        // to map this property to a physical physical column in the SQL database. 
        // This is the standard approach for read-only calculated/computed properties that 
        // exist purely for application-level convenience.
        [NotMapped]
        public string Name { get { return $"{FirstName} {LastName}"; } }

        #endregion

        #region Navigation Properties (Relationships)

        // Reference Note: Navigation properties for Many-to-Many relationships.
        // 1. [JsonIgnore] prevents circular reference serialization errors (System.Text.Json.JsonException) 
        //    when returning this entity directly via a Web API.
        // 2. The 'virtual' keyword allows EF Core to dynamically override the property to support 
        //    Lazy-Loading Proxies (if the proxy package is installed and configured).

        // Navigation property for the implicit/automatic Many-to-Many relationship mapping.
        [JsonIgnore]
        public virtual List<Book> Books { get; set; } = [];

        // Navigation property for the explicit Join Entity (Payload Table) Many-to-Many mapping.
        [JsonIgnore]
        public virtual List<BookAuthorMaps> BookAuthorMaps { get; set; } = [];

        #endregion
    }
}