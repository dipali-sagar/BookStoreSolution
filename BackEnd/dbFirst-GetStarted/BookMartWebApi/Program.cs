using BookMart.Model.ConfigModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BookMartApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                //c.OperationFilter<AddTenantIdFromHeader>();
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                // This stops the infinite loop by ignoring the back-reference
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                // Optional: Makes the JSON easier to read in the browser
                options.JsonSerializerOptions.WriteIndented = true;
            });

            builder.Services.Configure<AppConfig>((x) =>
            {
                x.DbConnectionString = builder?.Configuration?.GetConnectionString("DefaultConnection") ?? string.Empty;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
