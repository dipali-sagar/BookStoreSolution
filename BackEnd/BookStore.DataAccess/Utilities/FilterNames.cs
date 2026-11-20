namespace BookStore.DataAccess.Utilities
{
    /// <summary>
    /// A centralized place to store Global Query Filter names.
    /// Using constants prevents "magic string" typos across the application 
    /// when you need to bypass filters using .IgnoreQueryFilters()
    /// </summary>
    public static class FilterNames
    {
        #region Global Query Filter Constants

        public const string SoftDelete = nameof(SoftDelete);

        public const string Tenant = nameof(Tenant);

        #endregion
    }
}