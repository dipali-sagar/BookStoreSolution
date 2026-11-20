using System;

namespace BookStore.DataAccess.Utilities
{
    /// <summary>
    /// This interface abstracts how we identify the current tenant 
    /// (e.g., reading an HTTP Header, parsing a JWT token, or a background job context).
    /// By injecting this into our DbContext, we can apply Global Query Filters for multi-tenancy 
    /// without tightly coupling our Data Access layer to ASP.NET Core's HttpContext!
    /// </summary>
    public interface ITenantService
    {
        #region Contract

        /// <summary>
        /// Retrieves the unique identifier for the current tenant.
        /// </summary>
        /// <returns>The Tenant's Guid, or null if no tenant context is found.</returns>
        Guid? GetCurrentTenantId();

        #endregion
    }
}