namespace BookStore.Model.Features
{
    /// <summary>
    /// "Marker" or "Contract" interfaces like IAuditable allow us to apply 
    /// cross-cutting concerns across our entire domain model. 
    /// </summary>
    public interface IAuditable
    {
        #region Audit Properties

        DateTime CreatedOn { get; set; }

        DateTime UpdatedOn { get; set; }

        int? CreatedBy { get; set; }

        #endregion
    }
}