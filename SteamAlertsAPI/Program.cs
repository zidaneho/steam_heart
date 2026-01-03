using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using SteamAlertsAPI.Controllers;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Services;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", builder =>
    {
        builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
}
builder.Services.AddDbContext<SteamAlertsContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddHttpClient<ISteamService, SteamService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.UseCors("AllowReact");

app.UseHttpsRedirection();

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
        var context = services.GetRequiredService<SteamAlertsContext>();
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




