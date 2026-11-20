using BookStore.DataAccess.Utilities;

namespace BookStoreApi.Utilities
{
    /// <summary>
    /// The TenantService is a Scoped service that extracts the 'Tenant-Id' 
    /// from the incoming HTTP Request headers. This ID is then passed to the 
    /// ApplicationDbContext to power Global Query Filters.
    /// </summary>
    public class TenantService : ITenantService
    {
        #region Private Fields

        private readonly Guid? _currentTenantId;

        #endregion

        #region Constructor

        public TenantService(IHttpContextAccessor accessor)
        {
            var context = accessor.HttpContext;
            if (context == null) return;

            // TryGetValue is safer than index access. 
            // We use TryParse to prevent a 500 error if the header is not a valid GUID.
            if (context.Request.Headers.TryGetValue("Tenant-Id", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader.ToString(), out var parsedGuid))
                {
                    _currentTenantId = parsedGuid;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the Tenant ID associated with the current request.
        /// Returns null if no header was provided or if the ID was invalid.
        /// </summary>
        public Guid? GetCurrentTenantId() => _currentTenantId;

        #endregion
    }
}