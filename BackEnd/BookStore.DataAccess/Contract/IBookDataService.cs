using BookStore.Model.EntityModels;
using BookStore.Model.RequestModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Contract
{
    /// <summary>
    /// The core data access contract for Book operations.
    /// By programming to this interface, the application can seamlessly swap between 
    /// Entity Framework Core, Dapper, and ADO.NET implementations during the presentation 
    /// without changing the underlying Controller logic.
    /// </summary>
    public interface IBookDataService
    {
        #region Standard CRUD & Tracking

        /// <summary>
        /// Retrieves all active books. Implementations will default to state-tracked entities if using an ORM.
        /// </summary>
        IEnumerable<Book> GetBooks();

        /// <summary>
        /// Retrieves books optimized for read-only scenarios (bypassing the ORM change tracker).
        /// </summary>
        IEnumerable<Book> GetBooksAsNoTracking();

        /// <summary>
        /// Retrieves a single book by its Primary Key.
        /// </summary>
        Book GetBookById(int id);

        /// <summary>
        /// Inserts a new book and returns the newly generated identity key.
        /// </summary>
        Task<int> Create(CreateBookRequest request);

        /// <summary>
        /// Updates the core fields of an existing book.
        /// </summary>
        Task Update(UpdateBookRequest request);

        /// <summary>
        /// Deletes a book. Defaults to a Soft Delete (IsDeleted = true) unless a Hard Delete is explicitly requested.
        /// </summary>
        void Delete(int id, bool isHardDelete = false);

        #endregion

        #region Demo: Loading Strategies

        /// <summary>
        /// Relies on proxy generation to load related data on-the-fly upon property access.
        /// </summary>
        IEnumerable<Book> GetBooks_LazyLoading();

        /// <summary>
        /// Front-loads all required related data in a single, optimized query (or split queries).
        /// </summary>
        IEnumerable<Book> GetBooks_EagerLoading();

        /// <summary>
        /// Manually triggers database calls to load related data for entities already tracked in memory.
        /// </summary>
        IEnumerable<Book> GetBooks_ExplicitLoading();

        #endregion

        #region Demo: Advanced Updates & Transactions

        /// <summary>
        /// Modifies specific properties without pulling the entire entity payload over the network.
        /// </summary>
        void Update_Partial(UpdateBookRequest request);

        /// <summary>
        /// Executes a raw SQL update command directly against the database engine.
        /// </summary>
        Task Update_RawSQL(UpdateBookRequest request);

        /// <summary>
        /// Demonstrates atomicity. Ensures that inserting a parent (Publisher) and a child (Book) 
        /// either succeed together or fail together using Database Transactions.
        /// </summary>
        Task AddBookWithNewPublisherAsync(Publisher publisher, Book book);

        #endregion

        #region Demo: Joins & Complex Queries

        /// <summary>
        /// Returns books that have exactly the specified number of co-authors.
        /// </summary>
        Task<List<Book>> GetBooksHavingAuthors(int authorCount);

        /// <summary>
        /// Groups books by their author count (e.g., "3 books have 1 author", "5 books have 2 authors").
        /// </summary>
        Task<dynamic> GetBooksByAuthorCount();

        /// <summary>
        /// Executes a LEFT JOIN. Returns all books, including self-published ones without a publisher.
        /// </summary>
        Task<dynamic> GetBookAndPublisher();

        /// <summary>
        /// Executes a RIGHT JOIN. Returns all publishers, including brand new ones with no books attached yet.
        /// </summary>
        Task<dynamic> GetPublisherAndBooks();

        /// <summary>
        /// Executes an INNER JOIN. Strictly returns books that have a confirmed publisher mapping.
        /// </summary>
        Task<dynamic> GetBookAndPublisherUsingJoin();

        #endregion

        #region Demo: Global Query Filters & Stored Procedures

        /// <summary>
        /// Bypasses the application's soft-delete filter to retrieve the "Recycle Bin".
        /// </summary>
        IEnumerable<Book> GetRecycledBooks();

        /// <summary>
        /// Reverses the soft-delete flag, bringing a record back to the active pool.
        /// </summary>
        Task RestoreBookAsync(int id);

        /// <summary>
        /// Retrieves data using a pre-compiled database Stored Procedure.
        /// </summary>
        Task<IEnumerable<Book>> GetBooksUsingSP();

        #endregion

        #region Demo: Vector Search (AI Semantic Integration)

        /// <summary>
        /// Iterates over existing records, generates AI semantic embeddings, and saves them back as SQL Server Vectors.
        /// </summary>
        Task SyncVectorSearchEmbedding();

        /// <summary>
        /// Converts a user's natural language query into an embedding and performs a nearest-neighbor distance calculation natively in SQL.
        /// </summary>
        Task<List<Book>> SearchBooksAsync(string searchQuery);

        #endregion

        #region Demo: Complex Object Graph Insertion
        Task<int> CreateBookGraphAsync(CreateBookRequest request);
        #endregion

        #region Demo: Indexer Properties Bag
        Task<int> CreateBookWithAttributeAsync(CreateBookRequest request);
        Task<List<Book>> GetLimitedEditionBooksAsync();
        #endregion
    }
}