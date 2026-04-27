using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Scalar.AspNetCore;
using SteamHeartAPI.Controllers;
using SteamHeartAPI.Data;
using SteamHeartAPI.Services;
// using Hangfire;
// using Hangfire.SqlServer;
// using Hangfire.Dashboard;
// using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddHangfire(configuration => configuration
//     .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
//     .UseSimpleAssemblyNameTypeSerializer()
//     .UseRecommendedSerializerSettings()
//     .UsePostgreSqlStorage(options =>
//     {
//         options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
//     }, new PostgreSqlStorageOptions
//     {
//         SchemaName = "steam_alerts_hangfire", // Custom schema name
//         PrepareSchemaIfNecessary = true,      // Automatically create tables
//         QueuePollInterval = TimeSpan.FromSeconds(15),
//         InvisibilityTimeout = TimeSpan.FromMinutes(5)
//     }));

// // 2. Add the processing server (this runs the background jobs)
// builder.Services.AddHangfireServer();

var allowedOrigins = builder.Configuration["AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(o => o.Trim())
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        // 2. Only allow specific origins
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Essential if you use Cookies/Auth tokens
        }
        else
        {
            // Fallback for safety: allow nothing if config is missing
            policy.WithOrigins("https://your-frontend-domain.com");
        }
    });
});

// Support DATABASE_URL env var (standard on most hosting platforms) as well as
// the ASP.NET Core nested-key form ConnectionStrings__DefaultConnection.
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No database connection string found. Set DATABASE_URL or ConnectionStrings__DefaultConnection.");
}

// Npgsql does not support postgresql:// URIs — convert to key=value format.
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);
    var connBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
        SslMode = Npgsql.SslMode.Require
    };
    connectionString = connBuilder.ConnectionString;
}

// Log which source was used (without the password) to help diagnose deployment issues.
var startupLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
var safeConnStr = System.Text.RegularExpressions.Regex.Replace(connectionString, @"(password=|:[^@]+@)", m => m.Value.StartsWith(":") ? ":***@" : "password=***");
startupLogger.LogInformation("Using connection string: {ConnStr}", safeConnStr);

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<SteamHeartContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddControllers();
builder.Services.AddHttpClient<ISteamService, SteamService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.MapOpenApi();

app.MapScalarApiReference(options => { options.Servers = []; });

//app.UseHangfireDashboard();

app.UseRouting();

app.UseCors("ProductionCors");

app.UseHttpsRedirection();

app.MapGet("/api/health", async (SteamHeartContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new { status = "ok", database = "connected" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "error", database = ex.Message }, statusCode: 500);
    }
}).ExcludeFromDescription();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

// // Use the Cron expression for "every hour"
// RecurringJob.AddOrUpdate<ISteamService>(
//     "hourly-steam-fetch",
//     service => service.FetchAllMetricsAsync(),
//     Cron.Weekly);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SteamHeartContext>();
        // This applies any pending migrations and creates the database if it doesn't exist
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating the DB.");
    }
}


app.Run();




