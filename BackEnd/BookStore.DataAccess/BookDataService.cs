using BookStore.DataAccess.Contract;
using BookStore.DataAccess.Utilities;
using BookStore.Model.EntityModels;
using BookStore.Model.Features;
using BookStore.Model.RequestModels;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using OpenAI.Embeddings;

namespace BookStore.DataAccess
{
    public class BookDataService : IBookDataService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantService _tenantService;
        private readonly EmbeddingClient _embeddingClient;
        private readonly IFeatureManager _featureManager;

        public BookDataService(
            ApplicationDbContext db,
            ITenantService tenantService,
            EmbeddingClient embeddingClient,
            IFeatureManager featureManager)
        {
            _db = db;
            _tenantService = tenantService;
            _embeddingClient = embeddingClient;
            _featureManager = featureManager;
        }

        #region Standard CRUD & Tracking

        /// <summary>
        /// Standard retrieval. Entities are tracked by the EF Core Change Tracker by default.
        /// </summary>
        public IEnumerable<Book> GetBooks()
        {
            return _db.Books.ToList();
        }

        /// <summary>
        /// Performance improvement for Read-Only operations. Bypasses the Change Tracker, 
        /// saving memory and CPU when you don't intend to update the fetched records.
        /// </summary>
        public IEnumerable<Book> GetBooksAsNoTracking()
        {
            return _db.Books.AsNoTracking().ToList();
        }

        public Book GetBookById(int id)
        {
            // DEMO NOTE: Find() checks the local EF Core cache first before hitting the database.
            // If the entity is already tracked, it saves a database round-trip.
            var book = _db.Books.Find(id);

            // Alternative: FirstOrDefault goes straight to the DB every time.
            // var book1 = _db.Books.FirstOrDefault(x => x.Id == id);

            return book;
        }

        public async Task<int> Create(CreateBookRequest request)
        {
            var book = new Book()
            {
                Price = request.Price,
                ISBN = request.ISBN,
                Title = request.Title,
                Description = request.Description,
            };

            // Generate semantic vector before saving to the database
            await UpdateVectorSearch(book);

            _db.Books.Add(book);
            _db.SaveChanges(); // ID is automatically populated here by EF Core

            return book.Id;
        }

        public async Task Update(UpdateBookRequest request)
        {
            var existingBook = _db.Books.Single(x => x.Id == request.Id);

            existingBook.Price = request.Price;
            existingBook.Title = request.Title;
            existingBook.ISBN = request.ISBN;
            existingBook.Description = request.Description;

            // Re-calculate the semantic vector because the description or title might have changed
            await UpdateVectorSearch(existingBook);

            await _db.SaveChangesAsync();
        }

        public void Delete(int id, bool isHardDelete = false)
        {
            // If it's a hard delete, we need to bypass global query filters to find the book 
            // even if it was previously marked as soft-deleted.
            var book = isHardDelete
                ? _db.Books.IgnoreQueryFilters().FirstOrDefault(b => b.Id == id)
                : _db.Books.Find(id);

            if (book != null)
            {
                if (isHardDelete)
                {
                    // Triggers the interception logic to issue a real SQL DELETE command
                    book.IsHardDeleteRequested = true;
                }

                // If not a hard delete, our SaveChangesInterceptor will catch this Remove() 
                // and convert it into an UPDATE (IsDeleted = true).
                _db.Books.Remove(book);
                _db.SaveChanges();
            }
        }

        #endregion

        #region Demo: Saving Indexer Properties

        public async Task<int> CreateBookWithAttributeAsync(CreateBookRequest request)
        {
            var book = new Book()
            {
                Price = request.Price,
                ISBN = request.ISBN,
                Title = request.Title,
                Description = request.Description,
            };

            // Assign values using the C# Indexer
            // EF Core knows that these string keys map to actual SQL columns!
            book["CustomCoverColor"] = "Neon Blue";
            book["IsLimitedEdition"] = true;
            book["PromoCodeId"] = 99;

            // Generate semantic vector before saving to the database
            await UpdateVectorSearch(book);

            _db.Books.Add(book);
            _db.SaveChanges(); // ID is automatically populated here by EF Core

            return book.Id;
        }

        #endregion

        #region Demo: Querying Indexer Properties

        public async Task<List<Book>> GetLimitedEditionBooksAsync()
        {
            // DEMO NOTE: Using EF.Property<T> gives your LINQ query 'X-Ray vision' 
            // into the indexer properties so it can translate them to a SQL WHERE clause.

            var limitedBooks = await _db.Books
                .Where(b => EF.Property<bool>(b, "IsLimitedEdition") == true)
                .ToListAsync();

            return limitedBooks;
        }

        #endregion

        #region Demo: Loading Strategies

        /// <summary>
        /// Lazy Loading: Defers loading of related entities until the property is explicitly accessed.
        /// </summary>
        public IEnumerable<Book> GetBooks_LazyLoading()
        {
            // Requires Microsoft.EntityFrameworkCore.Proxies package 
            // and UseLazyLoadingProxies() in Program.cs (or custom ILazyLoader injection)
            return _db.Books.ToList();
        }

        /// <summary>
        /// Eager Loading: Loads all specified related data in a single initial database query.
        /// </summary>
        public IEnumerable<Book> GetBooks_EagerLoading()
        {
            // Eager Loading using Include / ThenInclude
            var query = _db.Books
                .Include(x => x.Publisher)
                .Include(x => x.BookDetails)
                .Include(x => x.BookAuthorMaps)
                    .ThenInclude(x => x.Author);

            // AsSplitQuery() breaks complex relational queries into multiple SQL statements 
            // to avoid Cartesian explosion (massive duplicate data returned over the network).
            var splitQuery = _db.Books
                .AsSplitQuery()
                .Include(x => x.BookAuthorMaps)
                    .ThenInclude(x => x.Author)
                .Include(x => x.Publisher)
                .Include(x => x.BookDetails);

            // Print SQL to console so you can show the audience during the demo!
            Console.WriteLine("--- Standard Eager Load SQL ---");
            Console.WriteLine(query.ToQueryString());

            Console.WriteLine("--- Split Query SQL ---");
            Console.WriteLine(splitQuery.ToQueryString());

            return query.ToList();
        }

        /// <summary>
        /// Explicit Loading: Manually requests related data for an entity that is already tracked.
        /// </summary>
        public IEnumerable<Book> GetBooks_ExplicitLoading()
        {
            var books = _db.Books.ToList();

            // Explicitly loading references/collections only when dictated by business logic
            foreach (var book in books)
            {
                _db.Entry(book).Reference(b => b.Publisher).Load();
                _db.Entry(book).Reference(b => b.BookDetails).Load();
                _db.Entry(book).Collection(b => b.BookAuthorMaps).Load();
            }

            return books;
        }

        #endregion

        #region Demo: Advanced Updates & Transactions

        /// <summary>
        /// Demonstrates updating specific columns without loading the full entity record first.
        /// </summary>
        public void Update_Partial(UpdateBookRequest request)
        {
            // 1. Create a stub entity with the primary key
            var existingBook = new Book { Id = request.Id, Price = request.Price };

            // 2. Attach it to the context (state becomes Unchanged)
            _db.Books.Attach(existingBook);

            // 3. Modify a property. Only Title will be marked as modified and sent in the SQL UPDATE.
            existingBook.Title = request.Title;
            _db.SaveChanges();
        }

        /// <summary>
        /// Demonstrates bypassing the Change Tracker entirely to execute raw, performant SQL.
        /// </summary>
        public async Task Update_RawSQL(UpdateBookRequest request)
        {
            // Note: ExecuteSqlInterpolatedAsync safely parameterizes inputs to prevent SQL injection.
            int affectedRows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Books 
                SET Price = {request.Price},
                    Title = {request.Title},
                    ISBN = {request.ISBN}
                WHERE Id = {request.Id}");
        }

        /// <summary>
        /// Explaining Explicit Transactions vs Implicit Graph Saving.
        /// </summary>
        public async Task AddBookWithNewPublisherAsync(Publisher publisher, Book book)
        {
            // APPROACH 1: Explicit Database Transaction
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    await _db.Publishers.AddAsync(publisher);
                    await _db.SaveChangesAsync(); // Generates Publisher ID from DB

                    book.PublisherId = publisher.Id; // Assign the newly generated foreign key
                    await _db.Books.AddAsync(book);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync(); // Finalize both operations
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(); // Revert all operations on failure
                    throw;
                }
            }

            /* // APPROACH 2: Implicit EF Core Graph Save (Uncomment to demo)
            // EF Core automatically wraps SaveChanges() in a transaction. It handles inserting the 
            // parent, grabbing the new ID, and inserting the child sequentially for you!
            
            book.Publisher = publisher;
            _db.Books.Add(book);
            await _db.SaveChangesAsync(); 
            */
        }

        #endregion

        #region Demo: Joins & Complex Queries

        public async Task<List<Book>> GetBooksHavingAuthors(int authorCount)
        {
            // Demonstrates querying against the Count property of a relational collection
            return await _db.Books
                .Include(b => b.BookAuthorMaps)
                    .ThenInclude(map => map.Author)
                .Where(b => b.BookAuthorMaps.Count == authorCount)
                .ToListAsync();
        }

        public async Task<dynamic> GetBooksByAuthorCount()
        {
            // Demonstrates Server-Side GroupBy evaluation
            return await _db.Books
                .Include(b => b.BookAuthorMaps)
                .GroupBy(b => b.BookAuthorMaps.Count)
                .Select(g => new
                {
                    NumberOfAuthors = g.Key,
                    Books = g.ToList()
                })
                .ToListAsync();
        }

        /// <summary>
        /// Demonstrates a LEFT JOIN to include books that might not have a publisher.
        /// </summary>
        public async Task<dynamic> GetBookAndPublisher()
        {
            return await _db.Books
                .LeftJoin(
                    _db.Publishers,
                    book => book.PublisherId,
                    pub => pub.Id,
                    (book, pub) => new
                    {
                        book.Title,
                        PublisherName = pub != null ? pub.Name : "Independent"
                    }
                ).ToListAsync();
        }

        /// <summary>
        /// Demonstrates a RIGHT JOIN to include publishers that might not have published any books yet.
        /// </summary>
        public async Task<dynamic> GetPublisherAndBooks()
        {
            return await _db.Books
                .RightJoin(
                    _db.Publishers,
                    book => book.PublisherId,
                    pub => pub.Id,
                    (book, pub) => new
                    {
                        BookTitle = book != null ? book.Title : "No books published yet",
                        PublisherName = pub.Name
                    }
                )
                .ToListAsync();
        }

        /// <summary>
        /// Demonstrates an INNER JOIN using LINQ Query Syntax vs Method Syntax.
        /// </summary>
        public async Task<dynamic> GetBookAndPublisherUsingJoin()
        {
            // Query Syntax
            var innerJoin = await (from b in _db.Books
                                   join p in _db.Publishers on b.PublisherId equals p.Id
                                   select new { b.Title, PublisherName = p.Name })
                                   .ToListAsync();

            /*
            // Method Syntax Equivalent
            var innerJoinUsingMethodSyntax = await _db.Books
                .Join(_db.Publishers,
                    b => b.PublisherId,
                    p => p.Id,
                    (b, p) => new { b.Title, PublisherName = p.Name })
                .ToListAsync();
            */

            return innerJoin;
        }

        #endregion

        #region Demo: Global Query Filters & Stored Procedures

        /// <summary>
        /// Demonstrates bypassing the built-in Soft Delete filter to fetch the "Recycle Bin".
        /// </summary>
        public IEnumerable<Book> GetRecycledBooks()
        {
            return _db.Books
                .IgnoreQueryFilters([FilterNames.SoftDelete])
                .Where(x => x.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// Reverses the Soft Delete flag.
        /// </summary>
        public async Task RestoreBookAsync(int id)
        {
            var book = await _db.Books
                .IgnoreQueryFilters([FilterNames.SoftDelete])
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted);

            if (book != null)
            {
                ((ISoftDeletable)book).Undo(); // Sets IsDeleted to false and clears DeletedOn
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Demonstrates calling a database Stored Procedure while injecting current context (TenantId).
        /// </summary>
        public async Task<IEnumerable<Book>> GetBooksUsingSP()
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            // Executing Stored Procedures
            // Note: Since the SP handles filtering internally, we must bypass EF Core's global filters 
            // to avoid double-filtering (which would crash if the SP doesn't return the TenantId column).
            var books = await _db.Books
                .FromSqlInterpolated($"EXEC [dbo].[sp_GetAllBooks] @TenantId = {currentTenantId}")
                .IgnoreQueryFilters()
                .ToListAsync();

            return books;
        }

        #endregion

        #region Demo: Vector Search (AI Semantic Integration)

        /// <summary>
        /// Generates a semantic embedding payload and maps it to EF Core 10's native SqlVector type.
        /// </summary>
        private async Task UpdateVectorSearch(Book book)
        {
            bool isVectorSearchEnabled = await _featureManager.IsEnabledAsync("VectorSearchEnabled");

            if (isVectorSearchEnabled)
            {
                // 1. Combine descriptive fields into a single rich text string
                string semanticPayload = $"Title: {book.Title}, ISBN: {book.ISBN}, Price: ${book.Price}, Description: {book.Description}";

                // 2. Generate the embedding from your OpenAI-compatible model (Local or Cloud)
                var embeddingResponse = await _embeddingClient.GenerateEmbeddingAsync(semanticPayload);

                // 3. Convert the response memory footprint directly into SQL Server's optimized vector struct
                ReadOnlyMemory<float> memoryVector = embeddingResponse.Value.ToFloats();
                book.SearchVector = new SqlVector<float>(memoryVector);
            }
        }

        /// <summary>
        /// Iterates over existing records, overriding query filters to ensure all records get a vector.
        /// </summary>
        public async Task SyncVectorSearchEmbedding()
        {
            var books = _db.Books.IgnoreQueryFilters().ToList();

            foreach (var book in books)
            {
                await UpdateVectorSearch(book);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Dynamically executes Native SQL Server nearest-neighbor searches or standard text fallbacks.
        /// </summary>
        public async Task<List<Book>> SearchBooksAsync(string searchQuery)
        {
            // 1. Check the Feature Flag
            bool isVectorSearchEnabled = await _featureManager.IsEnabledAsync("VectorSearchEnabled");

            if (isVectorSearchEnabled)
            {
                // 2. Translate the user's natural language query into an embedding vector
                var embeddingResponse = await _embeddingClient.GenerateEmbeddingAsync(searchQuery);

                // 3. Convert to EF Core 10's native SQL Server vector type
                var queryVector = new SqlVector<float>(embeddingResponse.Value.ToFloats());

                // 4. Perform Vector Search directly in SQL Server
                // EF.Functions.VectorDistance calculates the mathematical distance natively on the DB engine
                return await _db.Books
                    .Where(b => b.SearchVector != null && !b.IsDeleted)
                    .OrderBy(b => EF.Functions.VectorDistance("cosine", b.SearchVector.Value, queryVector))
                    .Take(3)
                    .ToListAsync();
            }
            else
            {
                // 5. Fallback Strategy: Traditional Keyword / SQL LIKE Search
                return await _db.Books
                    .Where(b => !b.IsDeleted &&
                               (b.Title.Contains(searchQuery) ||
                                b.ISBN.Contains(searchQuery) ||
                                b.Description.Contains(searchQuery)))
                    .Take(5)
                    .ToListAsync();
            }
        }

        #endregion

        #region Demo: Complex Object Graph Insertion

        public async Task<int> CreateBookGraphAsync(CreateBookRequest request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Initialize the Base Entity
                var book = new Book
                {
                    Title = request.Title,
                    ISBN = request.ISBN,
                    Price = request.Price,
                    Description = request.Description
                };

                // 2. Handle Publisher (One-to-Many)
                if (request.ExistingPublisherId.HasValue)
                {
                    book.PublisherId = request.ExistingPublisherId.Value;
                }
                else if (request.NewPublisher != null)
                {
                    book.Publisher = new Publisher
                    {
                        Name = request.NewPublisher.Name,
                        EstablishedYear = request.NewPublisher.EstablishedYear,
                        Headquarters = request.NewPublisher.Headquarters ?? "Unknown",
                        WebsiteUrl = request.NewPublisher.WebsiteUrl ?? "",
                        Description = request.NewPublisher.Description ?? ""
                    };
                }

                // 3. Handle Book Details (One-to-One)
                if (request.BookDetails != null)
                {
                    book.BookDetails = new BookDetails
                    {
                        NumberOfPages = request.BookDetails.NumberOfPages,
                        NumberOfChapters = request.BookDetails.NumberOfChapters,
                        Format = request.BookDetails.Format,
                        Language = request.BookDetails.Language,
                        ItemWeightInGrams = request.BookDetails.ItemWeightInGrams
                    };
                }

                // 4. Handle Authors with Payload (Many-to-Many via Join Entity)

                // A. Attach existing authors with new payload data
                if (request.ExistingAuthorMappings != null && request.ExistingAuthorMappings.Any())
                {
                    foreach (var mapping in request.ExistingAuthorMappings)
                    {
                        // Notice we don't need to fetch the Author from the DB first.
                        // We simply assign the AuthorId to the join entity, and EF Core handles the relationship!
                        book.BookAuthorMaps.Add(new BookAuthorMaps
                        {
                            AuthorId = mapping.AuthorId,
                            AuthorOrder = mapping.AuthorOrder,
                            ContributionRole = mapping.ContributionRole,
                            Description = mapping.Description
                        });
                    }
                }

                // B. Create brand new authors AND attach payload data
                if (request.NewAuthorMappings != null && request.NewAuthorMappings.Any())
                {
                    foreach (var mapping in request.NewAuthorMappings)
                    {
                        // Here we create the full Author object AND the Map object.
                        // EF Core will insert the Author, grab the ID, and apply it to the Map!
                        book.BookAuthorMaps.Add(new BookAuthorMaps
                        {
                            AuthorOrder = mapping.AuthorOrder,
                            ContributionRole = mapping.ContributionRole,
                            Description = mapping.Description,
                            Author = new Author
                            {
                                FirstName = mapping.Author.FirstName,
                                LastName = mapping.Author.LastName,
                                Email = mapping.Author.Email ?? "no-email@test.com",
                                Expertise = mapping.Author.Expertise ?? "General",
                                Biography = mapping.Author.Biography ?? "",
                                WebsiteUrl = mapping.Author.WebsiteUrl ?? ""
                            }
                        });
                    }
                }

                // 5. Generate AI Vector
                await UpdateVectorSearch(book);

                // 6. Execute the Graph Insert
                _db.Books.Add(book);
                await _db.SaveChangesAsync();

                // 7. Commit
                await transaction.CommitAsync();

                return book.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}