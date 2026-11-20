using BookStore.DataAccess.Contract;
using BookStore.Model.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    /// <summary>
    /// Core architectural controller demonstrating Standard CRUD, EF Core 10 optimizations, 
    /// Advanced Query Strategies, and Native SQL Server Vector Search.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("Books Management & EF Core Demonstrations")]
    public class BooksController : ControllerBase
    {
        private readonly IBookDataService _bookDataService;

        public BooksController(IBookDataService bookDataService)
        {
            _bookDataService = bookDataService;
        }

        #region Standard CRUD Operations

        /// <summary>
        /// Retrieves all active records (respects global tenant and soft-delete filters).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllBooks()
        {
            return Ok(_bookDataService.GetBooks());
        }

        /// <summary>
        /// Retrieves a single book entity by its unique identifier.
        /// </summary>
        [HttpGet("{id:int}", Name = "GetBookByIdRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetBook(int id)
        {
            var book = _bookDataService.GetBookById(id);
            if (book == null) return NotFound(new { Message = $"Book with ID {id} not found." });

            return Ok(book);
        }

        /// <summary>
        /// Creates a new book record within the active tenant context.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var bookId = await _bookDataService.Create(request);
            return CreatedAtAction(nameof(GetBook), new { id = bookId }, request);
            //return CreatedAtRoute("GetBookByIdRoute", new { id = bookId }, request);
        }

        /// <summary>
        /// Creates a new book record within the active tenant context.
        /// </summary>
        [HttpPost("demo/create/complex")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBookComplex([FromBody] CreateBookRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var bookId = await _bookDataService.CreateBookGraphAsync(request);
            return CreatedAtAction(nameof(GetBook), new { id = bookId }, request);
        }

        /// <summary>
        /// Updates a full book record via standard state tracking tracking modification.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            if (id != request.Id) return BadRequest(new { Error = "Route ID and Body ID mismatch" });

            await _bookDataService.Update(request);
            return Ok(new { Message = "Book updated successfully." });
        }

        /// <summary>
        /// Disposes of a book entry. Defaults to safe soft-deletion unless hard delete is explicitly requested.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id, [FromQuery] bool isHardDelete = false)
        {
            var book = _bookDataService.GetBookById(id);
            if (book == null) return NotFound(new { Message = $"Book with ID {id} not found." });

            _bookDataService.Delete(id, isHardDelete);
            return NoContent();
        }

        #endregion

        #region Demo: Loading Strategies

        /// <summary>
        /// DEMO: Resolves relations strictly on-demand using proxy/ILazyLoader boundaries.
        /// </summary>
        [HttpGet("demo/loading/lazy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetBooksLazy()
            => Ok(_bookDataService.GetBooks_LazyLoading());

        /// <summary>
        /// DEMO: Pulls dependencies upfront using .Include() to optimize database round-trips.
        /// </summary>
        [HttpGet("demo/loading/eager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetBooksEager()
            => Ok(_bookDataService.GetBooks_EagerLoading());

        /// <summary>
        /// DEMO: Manually executes targeted relational network loads on pre-tracked parent instances.
        /// </summary>
        [HttpGet("demo/loading/explicit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetBooksExplicit()
            => Ok(_bookDataService.GetBooks_ExplicitLoading());

        #endregion

        #region Demo: Change Tracking

        /// <summary>
        /// DEMO: Bypasses state manager snapshotting to achieve pure read-only performance loops.
        /// </summary>
        [HttpGet("demo/tracking/no-tracking")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetBooksNoTracking()
            => Ok(_bookDataService.GetBooksAsNoTracking());

        #endregion

        #region Demo: Raw SQL & Stored Procedures

        /// <summary>
        /// DEMO: Bridges abstract LINQ layers directly to raw database Stored Procedures.
        /// </summary>
        [HttpGet("demo/sql/stored-procedure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooksUsingSP()
            => Ok(await _bookDataService.GetBooksUsingSP());

        /// <summary>
        /// DEMO: Bypasses typical entity materialization loops to execute raw relational update scripts.
        /// </summary>
        [HttpPut("demo/sql/raw-update/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBookRawSql(int id, [FromBody] UpdateBookRequest request)
        {
            if (id != request.Id) return BadRequest(new { Error = "ID mismatch" });

            await _bookDataService.Update_RawSQL(request);
            return Ok(new { Message = "Raw SQL command executed." });
        }

        #endregion

        #region Demo: Joins & Complex Queries

        /// <summary>
        /// DEMO: Implements explicit relational mapping using flat, non-nullable linear Inner Joins.
        /// </summary>
        [HttpGet("demo/joins/inner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookAndPublisherJoin()
            => Ok(await _bookDataService.GetBookAndPublisherUsingJoin());

        /// <summary>
        /// DEMO: Returns all active books alongside optional publisher nodes via a Left Join layout.
        /// </summary>
        [HttpGet("demo/joins/left")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookAndPublisher()
            => Ok(await _bookDataService.GetBookAndPublisher());

        /// <summary>
        /// DEMO: Evaluates relational maps originating from the publisher parent collection outward.
        /// </summary>
        [HttpGet("demo/joins/right")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublisherAndBooks()
            => Ok(await _bookDataService.GetPublisherAndBooks());

        /// <summary>
        /// DEMO: Implements custom logical count aggregations across nested relational matrices.
        /// </summary>
        [HttpGet("demo/queries/authors-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAuthorsCount([FromQuery] int minAuthors = 1)
            => Ok(await _bookDataService.GetBooksHavingAuthors(minAuthors));

        /// <summary>
        /// DEMO: Compiles grouping statistics displaying books categorized by their total author count.
        /// </summary>
        [HttpGet("demo/queries/author-distribution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuthorCountDistribution()
            => Ok(await _bookDataService.GetBooksByAuthorCount());

        #endregion

        #region Demo: Global Query Filters & Soft Deletion

        /// <summary>
        /// DEMO: Temporarily ignores native architecture query limits to pull soft-deleted items.
        /// </summary>
        [HttpGet("demo/soft-delete/trashed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetTrashedBooks()
        {
            return Ok(_bookDataService.GetRecycledBooks());
        }

        /// <summary>
        /// DEMO: Clears soft-delete tracking variables to reinstate target records into the live pool.
        /// </summary>
        [HttpPost("demo/soft-delete/restore/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreBook(int id)
        {
            await _bookDataService.RestoreBookAsync(id);
            return Ok(new { Message = $"Book {id} successfully restored from recycle bin." });
        }

        #endregion

        #region Demo: Partial Updates

        /// <summary>
        /// DEMO: Modifies targeted scalar data tracking parameters without writing whole row updates.
        /// </summary>
        [HttpPut("demo/updates/partial/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateBookPartial(int id, [FromBody] UpdateBookRequest request)
        {
            if (id != request.Id) return BadRequest(new { Error = "ID mismatch" });

            _bookDataService.Update_Partial(request);
            return Ok(new { Message = "Partial structural property synchronization complete." });
        }

        #endregion

        #region Demo: Trigger Indexer Insert

        /// <summary>
        /// Triggers the service method that explicitly uses C# Indexers to populate 
        /// database columns (CustomCoverColor, IsLimitedEdition, PromoCodeId) 
        /// that do not exist as standard properties on the entity.
        /// </summary>
        [HttpPost("demo/indexer/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDemoBookWithAttribute([FromBody]CreateBookRequest request)
        {
            await _bookDataService.CreateBookWithAttributeAsync(request);

            return Ok(new
            {
                Message = "Successfully inserted a record using EF Core Indexer properties! Check your Serilog console to see the generated SQL INSERT statement."
            });
        }

        #endregion

        #region Demo: Trigger Indexer Query

        /// <summary>
        /// Retrieves records by querying an indexer property using EF.Property<T>().
        /// </summary>
        [HttpGet("demo/indexer/limited-editions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLimitedEditionBooks()
        {
            var results = await _bookDataService.GetLimitedEditionBooksAsync();

            // DEMO NOTE: When ASP.NET Core serializes this to JSON, the standard properties will serialize normally. However, the dynamic indexer data won't automatically serialize unless you specifically shape the response or implement a custom JSON converter for the Book class!

            return Ok(results);
        }

        #endregion

        #region Demo: VectorSearch (AI Semantic Integration)

        /// <summary>
        /// AI DEMO: Iterates over records to compile text payloads and update the database with native vector embeddings.
        /// </summary>
        [HttpPost("sync-vectors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SyncVectorEmbeddings()
        {
            try
            {
                await _bookDataService.SyncVectorSearchEmbedding();
                return Ok(new { Message = "Vector search embedding coordinates successfully generated and saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Vector generation pipeline crash", Details = ex.Message });
            }
        }

        /// <summary>
        /// AI DEMO: Leverages local OpenAI-compatible models to perform real-time conceptual vector matches against SQL Server.
        /// Example:
        /// 1. galactic travel and space anomalies
        /// 2. How to write maintainable code and clean software systems
        /// 3. building good routines and breaking bad personal cycles
        /// 4. space anomalies
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { Error = "Search request cannot be empty or contain only whitespace characters." });
            }

            try
            {
                var searchResults = await _bookDataService.SearchBooksAsync(query);

                // Project clean response structures to bypass complex circular entity graphs
                var response = searchResults.Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.ISBN,
                    b.Price,
                    b.Description
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Semantic vector calculations failed", Details = ex.Message });
            }
        }

        #endregion
    }
}