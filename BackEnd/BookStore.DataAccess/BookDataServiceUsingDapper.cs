using BookStore.DataAccess.Contract;
using BookStore.Model.ConfigModels;
using BookStore.Model.EntityModels;
using BookStore.Model.RequestModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OpenAI.Embeddings;
using System.Data;

namespace BookStore.DataAccess
{
    /// <summary>
    /// Service demonstrating Dapper as a Micro-ORM. 
    /// Highlights the balance between pure ADO.NET performance and the convenience of automatic object mapping.
    /// </summary>
    public class BookDataServiceUsingDapper : IBookDataService
    {
        private readonly string _connectionString;
        private readonly EmbeddingClient _embeddingClient;
        private readonly IFeatureManager _featureManager;

        public BookDataServiceUsingDapper(
            IOptions<AppConfig> settings,
            EmbeddingClient embeddingClient,
            IFeatureManager featureManager)
        {
            _connectionString = settings.Value.DbConnectionString;
            _embeddingClient = embeddingClient;
            _featureManager = featureManager;
        }

        // Helper to quickly spin up a new SQL Connection. 
        // Note: Dapper extends IDbConnection natively, so we return IDbConnection here.
        private IDbConnection Connection => new SqlConnection(_connectionString);

        #region Standard CRUD Operations

        /// <summary>
        /// Retrieves records via raw SQL but eliminates ADO.NET mapping boilerplate. 
        /// Dapper automatically maps matching SQL column names to C# class properties.
        /// </summary>
        public IEnumerable<Book> GetBooks()
        {
            using (IDbConnection connection = Connection)
            {
                var query = "SELECT * FROM [dbo].[Books] WHERE IsDeleted = 0";
                return connection.Query<Book>(query);
            }
        }

        /// <summary>
        /// Retrieves a single record. Demonstrates Dapper's elegant parameterization using anonymous objects.
        /// </summary>
        public Book GetBookById(int id)
        {
            using (IDbConnection connection = Connection)
            {
                // Anonymous objects (new { Id = id }) safely map to SQL @Id parameters, preventing SQL injection
                var query = "SELECT * FROM [dbo].[Books] (NOLOCK) WHERE Id = @Id AND IsDeleted = 0";
                return connection.QueryFirstOrDefault<Book>(query, new { Id = id });
            }
        }

        /// <summary>
        /// Creates a record and returns the generated ID.
        /// </summary>
        public async Task<int> Create(CreateBookRequest request)
        {
            using (IDbConnection connection = Connection)
            {
                // DEMO NOTE: We use 'OUTPUT INSERTED.Id', which is modern SQL syntax 
                // to return values from the row that was just created.
                var query = @"INSERT INTO [dbo].[Books] 
                        ([Title], [ISBN], [Price], [Description], [SearchVector], [IsDeleted], [CreatedOn], [UpdatedOn], [TenantId])
                      OUTPUT INSERTED.Id
                      VALUES 
                        (@Title, @ISBN, @Price, @Description, @SearchVector, 0, @CreatedOn, @UpdatedOn, @TenantId)";

                var parameters = new
                {
                    request.Title,
                    request.ISBN,
                    request.Price,
                    request.Description,
                    SearchVector = await GenerateVectorSearch(request),
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    TenantId = Guid.NewGuid() // Usually injected from ITenantService
                };

                // QuerySingleAsync<int> tells Dapper to execute the query 
                // and map the single returned value (the ID) directly to an integer.
                return await connection.QuerySingleAsync<int>(query, parameters);
            }
        }

        /// <summary>
        /// Updates a record by mapping an anonymous object to parameterized SQL.
        /// </summary>
        public async Task Update(UpdateBookRequest request)
        {
            using (IDbConnection connection = Connection)
            {
                var query = @"UPDATE [dbo].[Books]
                              SET [Title] = @Title, [ISBN] = @ISBN, [Price] = @Price, [Description] = @Description, [SearchVector] = @SearchVector, [UpdatedOn] = @UpdatedOn
                              WHERE Id = @Id";

                var parameters = new
                {
                    request.Id,
                    request.Title,
                    request.ISBN,
                    request.Price,
                    request.Description,
                    SearchVector = await GenerateVectorSearch(request),
                    UpdatedOn = DateTime.UtcNow
                };

                await connection.ExecuteAsync(query, parameters);
            }
        }

        /// <summary>
        /// Hard deletes a record via simple execution.
        /// </summary>
        public void Delete(int id, bool isHardDelete = false)
        {
            using (IDbConnection connection = Connection)
            {
                var query = "DELETE FROM [dbo].[Books] WHERE Id = @Id";
                connection.Execute(query, new { Id = id });
            }
        }

        #endregion

        #region Demo: Advanced Updates & Transactions

        /// <summary>
        /// Dapper handles partial updates beautifully by simply limiting the SQL string and the anonymous parameter object.
        /// </summary>
        public void Update_Partial(UpdateBookRequest request)
        {
            using (IDbConnection connection = Connection)
            {
                var query = "UPDATE [dbo].[Books] SET [Title] = @Title WHERE Id = @Id";
                connection.Execute(query, new { request.Id, request.Title });
            }
        }

        /// <summary>
        /// Since Dapper is a raw SQL executor, this looks identical to standard updates.
        /// </summary>
        public async Task Update_RawSQL(UpdateBookRequest request)
        {
            using (IDbConnection connection = Connection)
            {
                // We can pass the whole 'request' object directly, and Dapper maps the matching property names to @Parameters!
                var query = "UPDATE [dbo].[Books] SET [Title] = @Title, [Price] = @Price, [ISBN] = @ISBN WHERE Id = @Id";
                await connection.ExecuteAsync(query, request);
            }
        }

        /// <summary>
        /// Demonstrates that Dapper works seamlessly with native ADO.NET Transactions, drastically reducing boilerplate code.
        /// </summary>
        public async Task AddBookWithNewPublisherAsync(Publisher publisher, Book book)
        {
            using (IDbConnection connection = Connection)
            {
                connection.Open();

                // DEMO NOTE: Dapper uses standard IDbTransactions efficiently.
                // Notice how much cleaner this is compared to the pure ADO.NET version!
                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert Publisher and get the new ID back using OUTPUT INSERTED.Id
                        string pubSql = "INSERT INTO Publishers (Name) OUTPUT INSERTED.Id VALUES (@Name)";
                        int publisherId = await connection.ExecuteScalarAsync<int>(pubSql, new { publisher.Name }, transaction);

                        // 2. Insert Book with the newly acquired PublisherId
                        string bookSql = @"INSERT INTO Books (Title, ISBN, Price, PublisherId, IsDeleted, CreatedOn, UpdatedOn, TenantId) 
                                           VALUES (@Title, @ISBN, @Price, @PublisherId, 0, @CreatedOn, @UpdatedOn, @TenantId)";

                        var parameters = new
                        {
                            book.Title,
                            book.ISBN,
                            book.Price,
                            PublisherId = publisherId,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                            book.TenantId
                        };

                        // Pass the transaction directly into Dapper's ExecuteAsync
                        await connection.ExecuteAsync(bookSql, parameters, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Demo: Joins & Complex Queries

        /// <summary>
        /// Demonstrates a relational grouping query. Returns directly mapped entities.
        /// </summary>
        public async Task<List<Book>> GetBooksHavingAuthors(int authorCount)
        {
            using (IDbConnection connection = Connection)
            {
                string sqlQuery = @"
                    SELECT b.* FROM Books b
                    INNER JOIN BookAuthorMaps bam ON b.Id = bam.BookId
                    WHERE b.IsDeleted = 0
                    GROUP BY b.Id, b.Title, b.ISBN, b.Price, b.PublisherId, b.IsDeleted, b.TenantId, b.CreatedOn, b.UpdatedOn, b.DeletedOn
                    HAVING COUNT(bam.AuthorId) = @AuthorCount";

                var result = await connection.QueryAsync<Book>(sqlQuery, new { AuthorCount = authorCount });
                return result.ToList();
            }
        }

        /// <summary>
        /// Retrieves hierarchical grouping data. Demonstrates Dapper mapping to `dynamic` types for flexible reads.
        /// </summary>
        public async Task<dynamic> GetBooksByAuthorCount()
        {
            using (IDbConnection connection = Connection)
            {
                string sqlQuery = @"
                    SELECT COUNT(bam.AuthorId) as AuthorCount, b.Id, b.Title
                    FROM Books b
                    LEFT JOIN BookAuthorMaps bam ON b.Id = bam.BookId
                    WHERE b.IsDeleted = 0
                    GROUP BY b.Id, b.Title";

                // Dapper maps completely unknown shapes to a dynamic list!
                var rawResults = await connection.QueryAsync<dynamic>(sqlQuery);

                // Group in memory via LINQ to match the EF Core shape
                return rawResults.GroupBy(x => (int)x.AuthorCount)
                                 .Select(g => new
                                 {
                                     NumberOfAuthors = g.Key,
                                     Books = g.Select(x => new Book { Id = x.Id, Title = x.Title }).ToList()
                                 }).ToList();
            }
        }

        public async Task<dynamic> GetBookAndPublisher()
        {
            using (IDbConnection connection = Connection)
            {
                string sqlQuery = @"
                    SELECT b.Title, ISNULL(p.Name, 'Independent') as PublisherName 
                    FROM Books b 
                    LEFT JOIN Publishers p ON b.PublisherId = p.Id";

                return await connection.QueryAsync<dynamic>(sqlQuery);
            }
        }

        public async Task<dynamic> GetPublisherAndBooks()
        {
            using (IDbConnection connection = Connection)
            {
                string sqlQuery = @"
                    SELECT ISNULL(b.Title, 'No books published yet') as BookTitle, p.Name as PublisherName 
                    FROM Books b 
                    RIGHT JOIN Publishers p ON b.PublisherId = p.Id";

                return await connection.QueryAsync<dynamic>(sqlQuery);
            }
        }

        public async Task<dynamic> GetBookAndPublisherUsingJoin()
        {
            using (IDbConnection connection = Connection)
            {
                string sqlQuery = @"
                    SELECT b.Title, p.Name as PublisherName 
                    FROM Books b 
                    INNER JOIN Publishers p ON b.PublisherId = p.Id";

                return await connection.QueryAsync<dynamic>(sqlQuery);
            }
        }

        #endregion

        #region Soft Delete & Stored Procedures

        /// <summary>
        /// Bypasses soft-delete application logic by explicitly querying IsDeleted = 1.
        /// </summary>
        public IEnumerable<Book> GetRecycledBooks()
        {
            using (IDbConnection connection = Connection)
            {
                var query = "SELECT * FROM [dbo].[Books] WHERE IsDeleted = 1";
                return connection.Query<Book>(query);
            }
        }

        /// <summary>
        /// Restores a soft-deleted record.
        /// </summary>
        public async Task RestoreBookAsync(int id)
        {
            using (IDbConnection connection = Connection)
            {
                var query = @"UPDATE [dbo].[Books] SET [IsDeleted] = 0, [DeletedOn] = NULL WHERE Id = @Id";
                await connection.ExecuteAsync(query, new { Id = id });
            }
        }

        /// <summary>
        /// Demonstrates how flawlessly Dapper handles Stored Procedures.
        /// </summary>
        public async Task<IEnumerable<Book>> GetBooksUsingSP()
        {
            using (IDbConnection connection = Connection)
            {
                // DEMO NOTE: Dapper calls stored procedures very elegantly using the CommandType parameter
                return await connection.QueryAsync<Book>(
                    "sp_GetAllBooks",
                    new { TenantId = Guid.NewGuid() /* Generally provided by ITenantService */ },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        #endregion

        #region Demo: Vector Search (AI Semantic Integration)

        /// <summary>
        /// Single-record vector embedding update. Relies on Dapper Custom Type Handlers.
        /// </summary>
        public async Task UpdateVectorSearch(Book book)
        {
            if (await _featureManager.IsEnabledAsync("VectorSearchEnabled"))
            {
                book.SearchVector = await GenerateVectorSearch(book);

                using var connection = new SqlConnection(_connectionString);
                string sql = "UPDATE Books SET SearchVector = @SearchVector WHERE Id = @Id";

                // DEMO NOTE: Dapper will automatically use our global SqlMapper.TypeHandler 
                // for the SearchVector (SqlVector<float>) property.
                await connection.ExecuteAsync(sql, new { book.SearchVector, book.Id });
            }
        }

        /// <summary>
        /// Selects all books and updates their vectors sequentially.
        /// </summary>
        public async Task SyncVectorSearchEmbedding()
        {
            using var connection = new SqlConnection(_connectionString);

            // EF's IgnoreQueryFilters() equivalent: simply select everything without a WHERE clause
            var books = await connection.QueryAsync<Book>("SELECT * FROM Books");

            foreach (var book in books)
            {
                await UpdateVectorSearch(book);
            }
        }

        /// <summary>
        /// Executes the native SQL Server VECTOR_DISTANCE nearest-neighbor query, perfectly mapped to a Book object.
        /// </summary>
        public async Task<List<Book>> SearchBooksAsync(string searchQuery)
        {
            using var connection = new SqlConnection(_connectionString);

            if (await _featureManager.IsEnabledAsync("VectorSearchEnabled"))
            {
                var embeddingResponse = await _embeddingClient.GenerateEmbeddingAsync(searchQuery);
                var queryVector = new SqlVector<float>(embeddingResponse.Value.ToFloats());

                string sql = @"
                SELECT TOP 5 * FROM Books 
                WHERE SearchVector IS NOT NULL AND IsDeleted = 0
                ORDER BY VECTOR_DISTANCE('cosine', SearchVector, @QueryVector)";

                var results = await connection.QueryAsync<Book>(sql, new { QueryVector = queryVector });
                return results.AsList();
            }
            else
            {
                string sql = @"
                SELECT TOP 5 * FROM Books 
                WHERE IsDeleted = 0 AND (
                    Title LIKE '%' + @SearchQuery + '%' OR 
                    ISBN LIKE '%' + @SearchQuery + '%' OR 
                    Description LIKE '%' + @SearchQuery + '%'
                )";

                var results = await connection.QueryAsync<Book>(sql, new { SearchQuery = searchQuery });
                return results.AsList();
            }
        }

        #endregion

        #region Private Helpers

        private async Task<SqlVector<float>?> GenerateVectorSearch(CreateBookRequest request)
        {
            return await GenerateVectorSearch(request.Title, request.ISBN, request.Price, request.Description);
        }

        private async Task<SqlVector<float>?> GenerateVectorSearch(UpdateBookRequest request)
        {
            return await GenerateVectorSearch(request.Title, request.ISBN, request.Price, request.Description);
        }

        private async Task<SqlVector<float>?> GenerateVectorSearch(Book book)
        {
            return await GenerateVectorSearch(book.Title, book.ISBN, book.Price ?? 0, book.Description);
        }

        private async Task<SqlVector<float>?> GenerateVectorSearch(string title, string? ISBN, decimal price, string? description)
        {
            if (await _featureManager.IsEnabledAsync("VectorSearchEnabled"))
            {
                string semanticPayload = $"Title: {title}, ISBN: {ISBN}, Price: ${price}, Description: {description}";
                var embeddingResponse = await _embeddingClient.GenerateEmbeddingAsync(semanticPayload);

                var searchVector = new SqlVector<float>(embeddingResponse.Value.ToFloats());

                return searchVector;
            }

            return null;
        }

        #endregion

        #region EF Core Specific Demos (Not Supported Native via Dapper)

        // DEMO NOTE: These methods throw exceptions intentionally to illustrate that Dapper is NOT an ORM.
        // It does not track state in memory, and complex relational loading is highly manual.

        /// <summary>
        /// Unsupported: Dapper does not use an EF Change Tracker. Every query is essentially "NoTracking".
        /// </summary>
        public IEnumerable<Book> GetBooksAsNoTracking()
            => throw new NotImplementedException("Dapper does not use a change tracker, so everything is 'NoTracking' by default.");

        /// <summary>
        /// Unsupported: Dapper does not generate dynamic proxy classes required for lazy loading.
        /// </summary>
        public IEnumerable<Book> GetBooks_LazyLoading()
            => throw new NotImplementedException("Dapper does not support proxy-based Lazy Loading.");

        /// <summary>
        /// Unsupported natively: While Dapper *can* do multi-mapping, EF Core-style `.Include()` relies on an underlying ORM engine.
        /// </summary>
        public IEnumerable<Book> GetBooks_EagerLoading()
            => throw new NotImplementedException("While Dapper can do multi-mapping, native EF Core-style Eager Loading (.Include) is an ORM feature.");

        /// <summary>
        /// Unsupported: Specific to Entity Framework's DbContext Entry state tracking.
        /// </summary>
        public IEnumerable<Book> GetBooks_ExplicitLoading()
            => throw new NotImplementedException("Explicit loading is specific to Entity Framework's DbContext.");

        #endregion

        #region Demo: Complex Object Graph Insertion

        /// <summary>
        /// Demonstrates how Dapper removes the parameter mapping boilerplate of pure ADO.NET, 
        /// while still requiring explicit SQL sequencing and transaction orchestration.
        /// </summary>
        public async Task<int> CreateBookGraphAsync(CreateBookRequest request)
        {
            using IDbConnection connection = Connection;
            connection.Open();

            // DEMO NOTE: Dapper hooks seamlessly into standard IDbTransactions
            using IDbTransaction transaction = connection.BeginTransaction();

            try
            {
                int publisherId = 0;
                var tenantId = Guid.NewGuid();
                var now = DateTime.UtcNow;

                // 1. Resolve Publisher
                if (request.ExistingPublisherId.HasValue)
                {
                    publisherId = request.ExistingPublisherId.Value;
                }
                else if (request.NewPublisher != null)
                {
                    string pubSql = @"INSERT INTO Publishers (Name, EstablishedYear, Headquarters, WebsiteUrl, Description) 
                                      OUTPUT INSERTED.Id 
                                      VALUES (@Name, @EstablishedYear, @Headquarters, @WebsiteUrl, @Description)";

                    // Notice how easily Dapper maps the nested DTO properties
                    publisherId = await connection.ExecuteScalarAsync<int>(pubSql, request.NewPublisher, transaction);
                }

                // 2. Insert Book
                string bookSql = @"INSERT INTO Books (Title, ISBN, Price, Description, SearchVector, PublisherId, IsDeleted, CreatedOn, UpdatedOn, TenantId) 
                                   OUTPUT INSERTED.Id 
                                   VALUES (@Title, @ISBN, @Price, @Description, @SearchVector, @PublisherId, 0, @CreatedOn, @UpdatedOn, @TenantId)";

                var bookParams = new
                {
                    request.Title,
                    request.ISBN,
                    request.Price,
                    request.Description,
                    SearchVector = await GenerateVectorSearch(request.Title, request.ISBN, request.Price, request.Description),
                    PublisherId = publisherId > 0 ? (int?)publisherId : null,
                    CreatedOn = now,
                    UpdatedOn = now,
                    TenantId = tenantId
                };

                int bookId = await connection.ExecuteScalarAsync<int>(bookSql, bookParams, transaction);

                // 3. Insert Book Details
                if (request.BookDetails != null)
                {
                    string detailsSql = @"INSERT INTO BookDetails (BookId, NumberOfPages, NumberOfChapters, Format, Language, ItemWeightInGrams) 
                                          VALUES (@BookId, @NumberOfPages, @NumberOfChapters, @Format, @Language, @ItemWeightInGrams)";

                    // We merge the BookId into the DTO data dynamically using an anonymous object
                    await connection.ExecuteAsync(detailsSql, new
                    {
                        BookId = bookId,
                        request.BookDetails.NumberOfPages,
                        request.BookDetails.NumberOfChapters,
                        request.BookDetails.Format,
                        request.BookDetails.Language,
                        request.BookDetails.ItemWeightInGrams
                    }, transaction);
                }

                // 4. Handle Authors (Many-to-Many via Join Entity)
                string mapSql = @"INSERT INTO BookAuthorMaps (BookId, AuthorId, AuthorOrder, ContributionRole, Description) 
                                  VALUES (@BookId, @AuthorId, @AuthorOrder, @ContributionRole, @Description)";

                // A. Attach Existing Authors
                if (request.ExistingAuthorMappings != null)
                {
                    foreach (var map in request.ExistingAuthorMappings)
                    {
                        await connection.ExecuteAsync(mapSql, new
                        {
                            BookId = bookId,
                            map.AuthorId,
                            map.AuthorOrder,
                            map.ContributionRole,
                            map.Description
                        }, transaction);
                    }
                }

                // B. Create New Authors and Map them
                if (request.NewAuthorMappings != null)
                {
                    string authorSql = @"INSERT INTO Authors (FirstName, LastName, Email, Expertise, Biography, WebsiteUrl) 
                                         OUTPUT INSERTED.Id 
                                         VALUES (@FirstName, @LastName, @Email, @Expertise, @Biography, @WebsiteUrl)";

                    foreach (var map in request.NewAuthorMappings)
                    {
                        // Dapper directly maps the map.Author DTO to the parameters
                        int newAuthorId = await connection.ExecuteScalarAsync<int>(authorSql, map.Author, transaction);

                        await connection.ExecuteAsync(mapSql, new
                        {
                            BookId = bookId,
                            AuthorId = newAuthorId,
                            map.AuthorOrder,
                            map.ContributionRole,
                            map.Description
                        }, transaction);
                    }
                }

                // 5. Commit Transaction
                transaction.Commit();
                return bookId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Demo: Saving Indexer Properties

        public async Task<int> CreateBookWithAttributeAsync(CreateBookRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Demo: Querying Indexer Properties

        public async Task<List<Book>> GetLimitedEditionBooksAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}