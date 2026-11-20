namespace BookStore.Model.ConfigModels
{
    /// <summary>
    /// Using the strongly-typed Options Pattern for configuration.
    /// This is a cleaner, safer, and more testable alternative to injecting 
    /// IConfiguration directly into your data access services.
    /// </summary>
    public class AppConfig
    {
        #region Configuration Properties

        // The 'required' modifier (C# 11) ensures this property MUST be initialized
        // when the object is created, preventing null reference exceptions at runtime 
        // if the connection string is missing from appsettings.json.
        public required string DbConnectionString { get; set; }

        #endregion
    }
}