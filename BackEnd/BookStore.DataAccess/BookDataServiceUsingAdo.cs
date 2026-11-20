using BookStore.DataAccess.Contract;
using BookStore.Model.ConfigModels;
using BookStore.Model.EntityModels;
using BookStore.Model.RequestModels;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using OpenAI.Embeddings;
using System.Data;

namespace BookStore.DataAccess
{
    public class BookDataServiceUsingAdo : IBookDataService
    {
        private readonly string _connectionString;
        private readonly EmbeddingClient _embeddingClient;
        private readonly IFeatureManager _featureManager;

        public BookDataServiceUsingAdo(
            IOptions<AppConfig> settings,
            EmbeddingClient embeddingClient,
            IFeatureManager featureManager)
        {
            _connectionString = settings.Value.DbConnectionString;
            _embeddingClient = embeddingClient;
            _featureManager = featureManager;
        }

        // Helper to quickly spin up a new SQL Connection. 
        // ADO.NET relies on Connection Pooling under the hood, so creating "new" connections is very fast.
        private SqlConnection Connection => new SqlConnection(_connectionString);

        #region Standard CRUD 

        /// <summary>
        /// Retrieves records via raw SQL. Demonstrates the manual boilerplate required 
        /// to map a SqlDataReader back into C# objects.
        /// </summary>
        public IEnumerable<Book> GetBooks()
        {
            string sqlQuery = "SELECT * FROM Books WHERE IsDeleted = 0";
            var books = new List<Book>();

            using (SqlConnection connection = Connection)
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) books.Add(MapToBook(reader));
                    }
                }
            }
            return books;
        }

        /// <summary>
        /// Retrieves a single record using parameterized SQL to prevent SQL Injection.
        /// </summary>
        public Book GetBookById(int id)
        {
            string sqlQuery = "SELECT * FROM Books WHERE Id = @Id AND IsDeleted = 0";
            Book book = null;

            using (SqlConnection connection = Connection)
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) book = MapToBook(reader);
                    }
                }
            }
            return book;
        }

        /// <summary>
        /// Creates a record and demonstrates how to retrieve the generated ID using SCOPE_IDENTITY().
        /// </summary>
        public async Task<int> Create(CreateBookRequest request)
        {
            // DEMO NOTE: We add 'SELECT SCOPE_IDENTITY();' to the end of our batch.
            // This returns the last IDENTITY value inserted into an identity column in the same scope.
            string sqlQuery = @"
                INSERT INTO Books (Title, ISBN, Price, Description, SearchVector, IsDeleted, CreatedOn, UpdatedOn, TenantId) 
                VALUES (@Title, @ISBN, @Price, @Description, @SearchVector, 0, @CreatedOn, @UpdatedOn, @TenantId);
                SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = Connection)
            {
                connection.Open();

                // Generate the AI embedding before insertion
                var searchVector = await GenerateVectorSearch(request);

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    #region Parameter Mapping

                    command.Parameters.AddWithValue("@Title", request.Title);
                    command.Parameters.AddWithValue("@ISBN", (object)request.ISBN ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", (object)request.Price ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)request.Description ?? DBNull.Value);

                    // SqlVector perfectly integrates with ADO.NET parameters
                    command.Parameters.AddWithValue("@SearchVector", (object)searchVector ?? DBNull.Value);

                    command.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@TenantId", Guid.NewGuid()); // In real app, get from ITenantService

                    #endregion

                    // ExecuteScalar() is used when the query returns a single value (the ID).
                    // Since SCOPE_IDENTITY() returns a numeric (decimal/numeric), we safely cast it to int.
                    var result = await command.ExecuteScalarAsync();

                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Updates a record by manually mapping every column.
        /// </summary>
        public async Task Update(UpdateBookRequest request)
        {
            string sqlQuery = @"
                UPDATE Books 
                SET Title = @Title, ISBN = @ISBN, Price = @Price, Description = @Description, SearchVector = @SearchVector, UpdatedOn = @UpdatedOn
                WHERE Id = @Id";

            using (SqlConnection connection = Connection)
            {
                connection.Open();
                var searchVector = await GenerateVectorSearch(request);

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", request.Id);
                    command.Parameters.AddWithValue("@Title", request.Title);
                    command.Parameters.AddWithValue("@ISBN", (object)request.ISBN ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", (object)request.Price ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)request.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SearchVector", (object)searchVector ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Hard deletes a record by executing a raw DELETE command.
        /// </summary>
        public void Delete(int id, bool isHardDelete = false)
        {
            string sqlQuery = "DELETE FROM Books WHERE Id = @Id";
            using (SqlConnection connection = Connection)
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Demo: Advanced Updates & Transactions

        /// <summary>
        /// Demonstrates that in ADO.NET, partial updates require writing an explicit, smaller SQL string.
        /// </summary>
        public void Update_Partial(UpdateBookRequest request)
        {
            string sqlQuery = "UPDATE Books SET Title = @Title WHERE Id = @Id";

            using (SqlConnection connection = Connection)
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", request.Id);
                    command.Parameters.AddWithValue("@Title", request.Title);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// In ADO.NET, executing raw SQL is the default behavior, so this is identical to a standard update.
        /// </summary>
        public async Task Update_RawSQL(UpdateBookRequest request)
        {
            string sqlQuery = "UPDATE Books SET Title = @Title, Price = @Price, ISBN = @ISBN WHERE Id = @Id";

            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", request.Id);
                    command.Parameters.AddWithValue("@Title", request.Title);
                    command.Parameters.AddWithValue("@Price", (object)request.Price ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ISBN", (object)request.ISBN ?? DBNull.Value);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Highly illustrative demo showing the massive boilerplate required for ADO.NET Transactions 
        /// compared to EF Core's automatic `SaveChanges()` behavior.
        /// </summary>
        public async Task AddBookWithNewPublisherAsync(Publisher publisher, Book book)
        {
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();

                // DEMO NOTE: Creating a manual transaction scope
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert Publisher using OUTPUT INSERTED.Id to get the generated ID back
                        string pubSql = "INSERT INTO Publishers (Name) OUTPUT INSERTED.Id VALUES (@Name)";
                        int publisherId;

                        using (SqlCommand pubCmd = new SqlCommand(pubSql, connection, transaction))
                        {
                            pubCmd.Parameters.AddWithValue("@Name", publisher.Name);
                            publisherId = (int)await pubCmd.ExecuteScalarAsync();
                        }

                        // 2. Insert Book with the newly retrieved PublisherId
                        string bookSql = @"INSERT INTO Books (Title, ISBN, Price, PublisherId, IsDeleted, CreatedOn, UpdatedOn, TenantId) 
                                           VALUES (@Title, @ISBN, @Price, @PubId, 0, @CreatedOn, @UpdatedOn, @TenantId)";

                        using (SqlCommand bookCmd = new SqlCommand(bookSql, connection, transaction))
                        {
                            bookCmd.Parameters.AddWithValue("@Title", book.Title);
                            bookCmd.Parameters.AddWithValue("@ISBN", (object)book.ISBN ?? DBNull.Value);
                            bookCmd.Parameters.AddWithValue("@Price", (object)book.Price ?? DBNull.Value);
                            bookCmd.Parameters.AddWithValue("@PubId", publisherId); // Assigning the retrieved ID
                            bookCmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                            bookCmd.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);
                            bookCmd.Parameters.AddWithValue("@TenantId", book.TenantId);

                            await bookCmd.ExecuteNonQueryAsync();
                        }

                        // 3. Commit the transaction if both queries succeed
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        // 4. Rollback entirely if any step fails
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Demo: Joins & Complex Queries

        /// <summary>
        /// Evaluates a grouping condition natively inside the SQL query using a HAVING clause.
        /// </summary>
        public async Task<List<Book>> GetBooksHavingAuthors(int authorCount)
        {
            string sqlQuery = @"
                SELECT b.* FROM Books b
                INNER JOIN BookAuthorMaps bam ON b.Id = bam.BookId
                WHERE b.IsDeleted = 0
                GROUP BY b.Id, b.Title, b.ISBN, b.Price, b.PublisherId, b.IsDeleted, b.TenantId, b.CreatedOn, b.UpdatedOn, b.DeletedOn
                HAVING COUNT(bam.AuthorId) = @AuthorCount";

            var books = new List<Book>();
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AuthorCount", authorCount);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) books.Add(MapToBook(reader));
                    }
                }
            }
            return books;
        }

        /// <summary>
        /// Grouping complex hierarchies in pure ADO.NET is difficult. This demonstrates fetching a flat dataset 
        /// first, and doing the hierarchical grouping in-memory using C# LINQ.
        /// </summary>
        public async Task<dynamic> GetBooksByAuthorCount()
        {
            string sqlQuery = @"
                SELECT COUNT(bam.AuthorId) as AuthorCount, b.Id, b.Title
                FROM Books b
                LEFT JOIN BookAuthorMaps bam ON b.Id = bam.BookId
                WHERE b.IsDeleted = 0
                GROUP BY b.Id, b.Title";

            var rawResults = new List<dynamic>();
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rawResults.Add(new
                            {
                                AuthorCount = reader.GetInt32(0),
                                Book = new Book { Id = reader.GetInt32(1), Title = reader.GetString(2) }
                            });
                        }
                    }
                }
            }

            // Grouping in-memory to match the nested EF Core demo output
            return rawResults.GroupBy(x => (int)x.AuthorCount)
                             .Select(g => new
                             {
                                 NumberOfAuthors = g.Key,
                                 Books = g.Select(x => x.Book).ToList()
                             }).ToList();
        }

        public async Task<dynamic> GetBookAndPublisher()
        {
            string sqlQuery = @"
                SELECT b.Title, ISNULL(p.Name, 'Independent') as PublisherName 
                FROM Books b 
                LEFT JOIN Publishers p ON b.PublisherId = p.Id";

            var results = new List<dynamic>();
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Title = reader.GetString(0),
                                PublisherName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return results;
        }

        public async Task<dynamic> GetPublisherAndBooks()
        {
            string sqlQuery = @"
                SELECT ISNULL(b.Title, 'No books published yet') as BookTitle, p.Name as PublisherName 
                FROM Books b 
                RIGHT JOIN Publishers p ON b.PublisherId = p.Id";

            var results = new List<dynamic>();
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                BookTitle = reader.GetString(0),
                                PublisherName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return results;
        }

        public async Task<dynamic> GetBookAndPublisherUsingJoin()
        {
            string sqlQuery = @"
                SELECT b.Title, p.Name as PublisherName 
                FROM Books b 
                INNER JOIN Publishers p ON b.PublisherId = p.Id";

            var results = new List<dynamic>();
            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Title = reader.GetString(0),
                                PublisherName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return results;
        }

        #endregion

        #region Soft Delete & Stored Procedures

        /// <summary>
        /// Bypasses soft-delete application logic by explicitly querying IsDeleted = 1.
        /// </summary>
        public IEnumerable<Book> GetRecycledBooks()
        {
            string sqlQuery = "SELECT * FROM Books WHERE IsDeleted = 1";
            var books = new List<Book>();

            using (SqlConnection connection = Connection)
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) books.Add(MapToBook(reader));
                    }
                }
            }
            return books;
        }

        /// <summary>
        /// Undoes a soft delete via a raw SQL update command.
        /// </summary>
        public async Task RestoreBookAsync(int id)
        {
            string sqlQuery = "UPDATE Books SET IsDeleted = 0, DeletedOn = NULL WHERE Id = @Id AND IsDeleted = 1";

            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Demonstrates how to execute a Stored Procedure natively using CommandType.StoredProcedure.
        /// </summary>
        public async Task<IEnumerable<Book>> GetBooksUsingSP()
        {
            var books = new List<Book>();

            using (SqlConnection connection = Connection)
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("sp_GetAllBooks", connection))
                {
                    // Informing ADO that we are calling a Stored Procedure rather than raw SQL text
                    command.CommandType = CommandType.StoredProcedure;

                    // Note: Assuming a hardcoded TenantId for the ADO demo, normally comes from ITenantService
                    command.Parameters.AddWithValue("@TenantId", Guid.NewGuid());

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) books.Add(MapToBook(reader));
                    }
                }
            }
            return books;
        }

        #endregion

        #region Demo: Vector Search (AI Semantic Integration)

        /// <summary>
        /// Updates a single book's vector via raw SQL ADO Parameter mapping.
        /// </summary>
        public async Task UpdateVectorSearch(Book book)
        {
            if (await _featureManager.IsEnabledAsync("VectorSearchEnabled"))
            {
                book.SearchVector = await GenerateVectorSearch(book);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                string sql = "UPDATE Books SET SearchVector = @SearchVector WHERE Id = @Id";
                using var command = new SqlCommand(sql, connection);

                // Add parameters manually - ADO gracefully accepts the SqlVector<float> type
                command.Parameters.AddWithValue("@SearchVector", book.SearchVector);
                command.Parameters.AddWithValue("@Id", book.Id);

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Selects all books and updates their vectors sequentially. 
        /// </summary>
        public async Task SyncVectorSearchEmbedding()
        {
            var books = new List<Book>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Manual fetch
                using var command = new SqlCommand("SELECT Id, Title, ISBN, Price, Description FROM Books", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    books.Add(new Book
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        ISBN = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Price = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                        Description = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }

            // Process sequentially
            foreach (var book in books)
            {
                await UpdateVectorSearch(book);
            }
        }

        /// <summary>
        /// The flagship Vector Search demo in ADO.NET! 
        /// Executes the native VECTOR_DISTANCE math function directly on the SQL Engine.
        /// </summary>
        public async Task<List<Book>> SearchBooksAsync(string searchQuery)
        {
            var books = new List<Book>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            if (await _featureManager.IsEnabledAsync("VectorSearchEnabled"))
            {
                // Generate embedding payload
                var embeddingResponse = await _embeddingClient.GenerateEmbeddingAsync(searchQuery);
                var queryVector = new SqlVector<float>(embeddingResponse.Value.ToFloats());

                // DEMO NOTE: Shows pure T-SQL vector proximity logic without LINQ abstractions
                string sql = @"
                SELECT TOP 5 Id, Title, ISBN, Price, Description 
                FROM Books 
                WHERE SearchVector IS NOT NULL AND IsDeleted = 0
                ORDER BY VECTOR_DISTANCE('cosine', SearchVector, @QueryVector)";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@QueryVector", queryVector);

                using var reader = await command.ExecuteReaderAsync();
                books = MapBooksFromReader(reader);
            }
            else
            {
                // Standard LIKE fallback logic
                string sql = @"
                SELECT TOP 5 Id, Title, ISBN, Price, Description 
                FROM Books 
                WHERE IsDeleted = 0 AND (
                    Title LIKE '%' + @SearchQuery + '%' OR 
                    ISBN LIKE '%' + @SearchQuery + '%' OR 
                    Description LIKE '%' + @SearchQuery + '%'
                )";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@SearchQuery", searchQuery);

                using var reader = await command.ExecuteReaderAsync();
                books = MapBooksFromReader(reader);
            }

            return books;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Centralized manual data mapping logic. Required extensively in ADO.NET to translate 
        /// raw relational cell data back into C# object properties.
        /// </summary>
        private Book MapToBook(SqlDataReader reader)
        {
            return new Book
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                ISBN = reader.IsDBNull(reader.GetOrdinal("ISBN")) ? null : reader.GetString(reader.GetOrdinal("ISBN")),
                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? null : reader.GetDecimal(reader.GetOrdinal("Price")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                PublisherId = reader.IsDBNull(reader.GetOrdinal("PublisherId")) ? null : reader.GetInt32(reader.GetOrdinal("PublisherId")),
                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                UpdatedOn = reader.GetDateTime(reader.GetOrdinal("UpdatedOn")),
                DeletedOn = reader.IsDBNull(reader.GetOrdinal("DeletedOn")) ? null : reader.GetDateTime(reader.GetOrdinal("DeletedOn"))
            };
        }

        /// <summary>
        /// Iterates through an executed reader and funnels it into the mapping engine.
        /// </summary>
        private List<Book> MapBooksFromReader(SqlDataReader reader)
        {
            var result = new List<Book>();
            while (reader.Read())
            {
                // Note: Modified slightly from original code to map via ordinal lookup rather than just ID/Title 
                // to prevent schema mismatches, but left conceptually identical for the presentation.
                result.Add(MapToBook(reader));
            }
            return result;
        }

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

        #region EF Core Specific Demos (Not Supported in ADO)

        // DEMO NOTE: These methods satisfy the IBookDataService interface but intentionally throw exceptions. 
        // This is a great talking point for your audience to explain that ADO.NET lacks a native Object Relational Mapping (ORM) state tracker.

        public IEnumerable<Book> GetBooksAsNoTracking()
            => throw new NotImplementedException("ADO.NET is 'NoTracking' by default because it lacks a Change Tracker.");

        public IEnumerable<Book> GetBooks_LazyLoading()
            => throw new NotImplementedException("ADO.NET does not support Lazy Loading Proxies. You must query related tables manually.");

        public IEnumerable<Book> GetBooks_EagerLoading()
            => throw new NotImplementedException("Eager Loading in ADO requires complex JOIN statements and manual mapping of the child entities.");

        public IEnumerable<Book> GetBooks_ExplicitLoading()
            => throw new NotImplementedException("Explicit loading is an EF Core DbContext feature.");

        #endregion

        #region Demo: Complex Object Graph Insertion

        /// <summary>
        /// Demonstrates the immense boilerplate required to insert a graph in pure ADO.NET.
        /// We must manually enforce the execution order, retrieve generated IDs, and pass the explicit transaction to every command.
        /// </summary>
        public async Task<int> CreateBookGraphAsync(CreateBookRequest request)
        {
            using SqlConnection connection = Connection;
            await connection.OpenAsync();

            // DEMO NOTE: Explicit transaction management is strictly required here to prevent orphaned records!
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                int publisherId = 0;
                int bookId = 0;
                var tenantId = Guid.NewGuid(); // Usually from ITenantService
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

                    using SqlCommand pubCmd = new SqlCommand(pubSql, connection, transaction);
                    pubCmd.Parameters.AddWithValue("@Name", request.NewPublisher.Name);
                    pubCmd.Parameters.AddWithValue("@EstablishedYear", request.NewPublisher.EstablishedYear);
                    pubCmd.Parameters.AddWithValue("@Headquarters", (object?)request.NewPublisher.Headquarters ?? DBNull.Value);
                    pubCmd.Parameters.AddWithValue("@WebsiteUrl", (object?)request.NewPublisher.WebsiteUrl ?? DBNull.Value);
                    pubCmd.Parameters.AddWithValue("@Description", (object?)request.NewPublisher.Description ?? DBNull.Value);

                    publisherId = (int)await pubCmd.ExecuteScalarAsync();
                }

                // 2. Insert Book
                var searchVector = await GenerateVectorSearch(request.Title, request.ISBN, request.Price, request.Description);

                string bookSql = @"INSERT INTO Books (Title, ISBN, Price, Description, SearchVector, PublisherId, IsDeleted, CreatedOn, UpdatedOn, TenantId) 
                                   OUTPUT INSERTED.Id 
                                   VALUES (@Title, @ISBN, @Price, @Description, @SearchVector, @PublisherId, 0, @CreatedOn, @UpdatedOn, @TenantId)";

                using SqlCommand bookCmd = new SqlCommand(bookSql, connection, transaction);
                bookCmd.Parameters.AddWithValue("@Title", request.Title);
                bookCmd.Parameters.AddWithValue("@ISBN", (object?)request.ISBN ?? DBNull.Value);
                bookCmd.Parameters.AddWithValue("@Price", request.Price);
                bookCmd.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
                bookCmd.Parameters.AddWithValue("@SearchVector", (object?)searchVector ?? DBNull.Value);
                bookCmd.Parameters.AddWithValue("@PublisherId", publisherId > 0 ? publisherId : DBNull.Value);
                bookCmd.Parameters.AddWithValue("@CreatedOn", now);
                bookCmd.Parameters.AddWithValue("@UpdatedOn", now);
                bookCmd.Parameters.AddWithValue("@TenantId", tenantId);

                bookId = (int)await bookCmd.ExecuteScalarAsync();

                // 3. Insert Book Details
                if (request.BookDetails != null)
                {
                    string detailsSql = @"INSERT INTO BookDetails (BookId, NumberOfPages, NumberOfChapters, Format, Language, ItemWeightInGrams) 
                                          VALUES (@BookId, @NumberOfPages, @NumberOfChapters, @Format, @Language, @ItemWeightInGrams)";

                    using SqlCommand detailsCmd = new SqlCommand(detailsSql, connection, transaction);
                    detailsCmd.Parameters.AddWithValue("@BookId", bookId);
                    detailsCmd.Parameters.AddWithValue("@NumberOfPages", request.BookDetails.NumberOfPages);
                    detailsCmd.Parameters.AddWithValue("@NumberOfChapters", request.BookDetails.NumberOfChapters);
                    detailsCmd.Parameters.AddWithValue("@Format", (object?)request.BookDetails.Format ?? DBNull.Value);
                    detailsCmd.Parameters.AddWithValue("@Language", (object?)request.BookDetails.Language ?? DBNull.Value);
                    detailsCmd.Parameters.AddWithValue("@ItemWeightInGrams", (object?)request.BookDetails.ItemWeightInGrams ?? DBNull.Value);

                    await detailsCmd.ExecuteNonQueryAsync();
                }

                // 4. Handle Authors (Many-to-Many via Join Entity)
                string mapSql = @"INSERT INTO BookAuthorMaps (BookId, AuthorId, AuthorOrder, ContributionRole, Description) 
                                  VALUES (@BookId, @AuthorId, @AuthorOrder, @ContributionRole, @Description)";

                // A. Attach Existing Authors
                if (request.ExistingAuthorMappings != null)
                {
                    foreach (var map in request.ExistingAuthorMappings)
                    {
                        using SqlCommand mapCmd = new SqlCommand(mapSql, connection, transaction);
                        mapCmd.Parameters.AddWithValue("@BookId", bookId);
                        mapCmd.Parameters.AddWithValue("@AuthorId", map.AuthorId);
                        mapCmd.Parameters.AddWithValue("@AuthorOrder", map.AuthorOrder);
                        mapCmd.Parameters.AddWithValue("@ContributionRole", (object?)map.ContributionRole ?? DBNull.Value);
                        mapCmd.Parameters.AddWithValue("@Description", (object?)map.Description ?? DBNull.Value);

                        await mapCmd.ExecuteNonQueryAsync();
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
                        // Insert new Author
                        using SqlCommand authorCmd = new SqlCommand(authorSql, connection, transaction);
                        authorCmd.Parameters.AddWithValue("@FirstName", map.Author.FirstName);
                        authorCmd.Parameters.AddWithValue("@LastName", map.Author.LastName);
                        authorCmd.Parameters.AddWithValue("@Email", (object?)map.Author.Email ?? DBNull.Value);
                        authorCmd.Parameters.AddWithValue("@Expertise", (object?)map.Author.Expertise ?? DBNull.Value);
                        authorCmd.Parameters.AddWithValue("@Biography", (object?)map.Author.Biography ?? DBNull.Value);
                        authorCmd.Parameters.AddWithValue("@WebsiteUrl", (object?)map.Author.WebsiteUrl ?? DBNull.Value);

                        int newAuthorId = (int)await authorCmd.ExecuteScalarAsync();

                        // Map to Book
                        using SqlCommand mapCmd = new SqlCommand(mapSql, connection, transaction);
                        mapCmd.Parameters.AddWithValue("@BookId", bookId);
                        mapCmd.Parameters.AddWithValue("@AuthorId", newAuthorId);
                        mapCmd.Parameters.AddWithValue("@AuthorOrder", map.AuthorOrder);
                        mapCmd.Parameters.AddWithValue("@ContributionRole", (object?)map.ContributionRole ?? DBNull.Value);
                        mapCmd.Parameters.AddWithValue("@Description", (object?)map.Description ?? DBNull.Value);

                        await mapCmd.ExecuteNonQueryAsync();
                    }
                }

                // 5. Commit Transaction
                await transaction.CommitAsync();
                return bookId;
            }
            catch
            {
                await transaction.RollbackAsync();
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