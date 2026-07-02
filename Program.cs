using Humanizer.Configuration;
using Microsoft.EntityFrameworkCore;
using StockFlow.API.Data;
using StockFlow.API.Extensions;
using StockFlow.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Updated with retry logic
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is missing. " +
        "Set it via 'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\"' for local development, " +
        "or via the ConnectionStrings__DefaultConnection environment variable in other environments.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Application services — Repositories, Services, Validators
builder.Services.AddApplicationServices();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger with JWT support
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
                  .GetRequiredService<ApplicationDbContext>();
    try
    {
        // This creates StockFlowDB if it doesn't exist
        // Then runs all migrations
        db.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
        throw;
    }
}
app.Run();
