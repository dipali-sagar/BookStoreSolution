using BookStore.DataAccess;
using BookStore.DataAccess.Contract;
using BookStore.DataAccess.Utilities;
using BookStore.Model.ConfigModels;
using BookStoreApi.Utilities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using OpenAI;
using OpenAI.Embeddings;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.ClientModel;
using System.Text.Json.Serialization;

namespace BookStoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1. Initialize Serilog BEFORE the WebApplication builder
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                // CRITICAL: Expose ONLY the EF Core SQL Commands
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                #region 1. Application Configuration [Optional]
                Log.Information("Starting web application...");
                builder.Host.UseSerilog();
                // Map strongly-typed configuration settings
                builder.Services.Configure<AppConfig>(options =>
                {
                    options.DbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
                });

                #endregion

                #region 2. Database & Entity Framework Setup

                // Registering the DbContext and specifying the database provider (SQL Server)
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    // If use PostreSQL
                    //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging() // Shows actual parameter values (e.g. @p0 = 'The Matrix')
                    .EnableDetailedErrors();       // Gives clearer exception messages if a query fails;

                    // (Lazy Loading): Uncomment the line below (and install Microsoft.EntityFrameworkCore.Proxies)
                    // to enable lazy loading across the application.
                    options.UseLazyLoadingProxies();
                });

                #endregion

                #region 3. Dependency Injection (Services)

                // Register custom services
                builder.Services.AddScoped<IBookDataService, BookDataService>();
                builder.Services.AddScoped<ITenantService, TenantService>();

                // Required to access HTTP Context (e.g., for headers in TenantService)
                builder.Services.AddHttpContextAccessor();

                #endregion

                #region 4. Controller & Serialization Configuration

                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        // (Serialization): Crucial when returning EF Core entities directly.
                        // This prevents infinite loop errors caused by bidirectional relationships 
                        // (e.g., Book -> Publisher -> Books -> Publisher...)
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                        // Keep the payload clean by omitting null values
                        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                        // Formatting JSON to make it readable during the live presentation
                        options.JsonSerializerOptions.WriteIndented = true;
                    });

                #endregion

                #region 5. Swagger / OpenAPI Configuration
                //builder.Services.AddOpenApi();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    // Custom filter to easily inject Tenant ID during the Swagger demo
                    c.OperationFilter<AddTenantIdFromHeader>();
                });

                #endregion

                #region 6. Vector Search Configuration
                builder.Services.AddFeatureManagement();

                builder.Services.AddSingleton<EmbeddingClient>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();

                    var endpoint = config["LocalAI:Endpoint"]!;
                    var apiKey = config["LocalAI:ApiKey"]!;
                    var modelId = config["LocalAI:EmbeddingModel"]!;

                    // Override the endpoint to point to your local server instead of OpenAI's cloud
                    var options = new OpenAIClientOptions
                    {
                        Endpoint = new Uri(endpoint)
                    };

                    var credential = new ApiKeyCredential(apiKey);
                    var openAiClient = new OpenAIClient(credential, options);

                    // Return the specific client responsible for embeddings
                    return openAiClient.GetEmbeddingClient(modelId);
                });
                #endregion

                SqlMapper.AddTypeHandler(new SqlVectorTypeHandler());

                var app = builder.Build();

                #region 6. HTTP Request Pipeline

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    //app.MapOpenApi();
                    app.MapSwagger("/openapi/{documentName}.json");
                    app.MapScalarApiReference();
                    //app.UseSwagger();
                    //app.UseSwaggerUI();
                }

                app.UseAuthorization();
                app.MapControllers();

                app.Run();

                #endregion
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}