using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BookStore.DataAccess.Utilities
{
    /// <summary>
    /// Design-Time Factory for Entity Framework Core Tooling.
    /// </summary>
    /// <remarks>
    /// DEMO NOTE: When you run `dotnet ef migrations add`, the EF CLI does not spin up the full 
    /// ASP.NET Core web host. Because our ApplicationDbContext relies on dependency injection 
    /// (e.g., ITenantService), the CLI will crash because it doesn't know how to provide that service.
    /// This factory acts as a "backdoor" exclusively for the CLI to safely instantiate the DbContext 
    /// without needing a running web server.
    /// 
    /// Currently kept for reference and advanced tooling scenarios.
    /// </remarks>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            #region 1. Build Configuration

            // DEMO NOTE: The EF CLI often runs from the DataAccess class library directory, 
            // but our connection strings live in the WebApi project. 
            // This setup manually traverses the file system to find the correct appsettings.json.
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)
                .AddJsonFile("BookStoreWebApi/appsettings.json")
                .AddJsonFile("BookStoreWebApi/appsettings.Development.json", optional: true)
                .Build();

            #endregion

            #region 2. Configure DbContext Options

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Retrieve connection string and configure the SQL Server provider natively.
            // If you were using Lazy Loading Proxies, you would also chain `.UseLazyLoadingProxies()` here.
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            // Swap out the provider for design-time CLI access
            //optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

            #endregion

            #region 3. Instantiate DbContext

            // Return the context. 
            // DEMO NOTE: Notice we are passing 'null' for the ITenantService. 
            // During design-time (migrations), we don't have an active HTTP request, an active user, 
            // or a multi-tenant context. Therefore, it is perfectly safe (and required) to bypass it here.
            return new ApplicationDbContext(optionsBuilder.Options, null);

            #endregion
        }
    }
}