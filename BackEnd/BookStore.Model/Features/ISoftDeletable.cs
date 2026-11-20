using System;

namespace BookStore.Model.Features
{
    /// <summary>
    /// This interface enables the "Soft Delete" pattern across our domain.
    /// When an entity implements this contract, our DbContext knows to intercept any 
    /// deletion attempts, flip the IsDeleted flag instead, and let the Global Query Filter 
    /// hide the record from normal application queries.
    /// </summary>
    public interface ISoftDeletable
    {
        #region Soft Delete Properties

        bool IsDeleted { get; set; }

        DateTime? DeletedOn { get; set; }

        #endregion

        #region Helper Methods

        // Optional helper to restore, Logic encapsulated within the entity for better DDD (Domain Driven Design).
        public void Undo()
        {
            IsDeleted = false;
            DeletedOn = null;
        }

        #endregion
}

    /// <summary>
    /// This is a "Bypass" interface. 
    /// When an entity implements this, it gains a temporary flag that tells our 
    /// DbContext Interceptor: "For this specific SaveChanges call, perform a 
    /// physical DELETE instead of a Soft Delete."
    /// </summary>
    public interface IHardDeletable
    {
        #region Hard Delete State

        /// <summary>
        /// This property is NOT mapped to the database. It exists only in memory 
        /// to communicate intent to the DbContext ChangeTracker.
        /// </summary>
        bool IsHardDeleteRequested { get; set; }

        #endregion
    }
}