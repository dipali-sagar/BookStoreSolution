using System;

namespace BookStore.Model.Features
{
    /// <summary>
    /// This interface is the cornerstone of our Multi-Tenancy implementation.
    /// Any entity that implements this contract will automatically have a TenantId assigned 
    /// during SaveChanges(), and will automatically be filtered by the current TenantId 
    /// via Global Query Filters on all read operations.
    /// </summary>
    public interface ITenantEntity
    {
        #region Tenant Properties

        Guid TenantId { get; set; }

        #endregion
    }
}