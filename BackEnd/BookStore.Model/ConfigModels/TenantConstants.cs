namespace BookStore.Model.ConfigModels
{
    /// <summary>
    /// Hardcoded Tenant IDs used for seeding data and manual testing.
    /// In a production environment, these would exist in a 'Tenants' table, 
    /// but for a training demo, having them as constants makes it easy to 
    /// simulate multi-tenant requests via Swagger or Postman headers.
    /// </summary>
    public static class TenantConstants
    {
        #region Hardcoded Tenant Identifiers

        // You can use these GUIDs in your X-Tenant-Id header 
        // to see the data filter in real-time.

        public readonly static Guid Crossword = new("864d14e2-615c-4d8f-85b6-24b521dd4bd9");

        public readonly static Guid Amazon = new("226dc44f-5a83-4d39-901e-87f7a790197b");

        #endregion
    }
}