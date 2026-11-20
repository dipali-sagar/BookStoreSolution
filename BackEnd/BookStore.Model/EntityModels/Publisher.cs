using System.Text.Json.Serialization;

namespace BookStore.Model.EntityModels
{
    /// <summary>
    /// This is the 'Principal' entity in a One-to-Many relationship.
    /// A single Publisher can be associated with multiple Books.
    /// </summary>
    public class Publisher
    {
        #region Core Data Properties

        // Primary Key automatically inferred by EF Core conventions.
        public int Id { get; set; }

        // Reference Note: The 'required' modifier enforces that these properties must be 
        // initialized during object creation. This ensures compile-time safety and prevents 
        // null reference exceptions before Entity Framework attempts to save to the database.
        public required string Name { get; set; }
        public int EstablishedYear { get; set; }
        public required string Headquarters { get; set; }
        public required string WebsiteUrl { get; set; }
        public required string Description { get; set; }

        #endregion

        #region Navigation Properties

        // Reference Note: Navigation property for the Principal side of a One-to-Many relationship.
        // 1. [JsonIgnore] prevents the JSON serializer from entering an infinite circular loop 
        //    (Publisher -> Book -> Publisher) when returning this entity directly from a Web API.
        // 2. The 'virtual' keyword allows EF Core to dynamically generate a proxy class at runtime 
        //    to support Lazy Loading (if the proxy package is installed and enabled).
        // 3. Initializing the collection (= []) is a modern C# best practice. It guarantees the 
        //    collection is never null, preventing NullReferenceExceptions when adding new items 
        //    to a newly instantiated Publisher object.

        // The 'virtual' keyword enables Lazy Loading Proxies.
        // We initialize the list to avoid NullReferenceExceptions when adding items.
        [JsonIgnore]
        public virtual List<Book> Books { get; set; } = [];

        #endregion
    }
}