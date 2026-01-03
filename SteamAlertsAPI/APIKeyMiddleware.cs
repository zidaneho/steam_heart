using Microsoft.Extensions.Primitives; // Required for headers

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string APIKEYNAME = "X-Api-Key"; // The header name clients must send

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    //https://your-api.onrender.com/scalar/v1?X-Api-Key=your-secret-key
    public async Task InvokeAsync(HttpContext context)
    {
        var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = appSettings.GetValue<string>("X_API_KEY");

        // 1. FAIL SAFE
        if (string.IsNullOrEmpty(apiKey))
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ApiKeyMiddleware>>();
            logger.LogWarning("Security Warning: X_API_KEY is missing...");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Server config error");
            return;
        }

        // 2. SEARCH FOR KEY (Header OR Query)
        string? extractedApiKey = null;

        if (context.Request.Headers.TryGetValue(APIKEYNAME, out var headerVal))
        {
            extractedApiKey = headerVal.ToString();
        }
        else if (context.Request.Query.TryGetValue(APIKEYNAME, out var queryVal))
        {
            extractedApiKey = queryVal.ToString();
        }

        // 3. IF MISSING IN BOTH PLACES -> 401
        if (string.IsNullOrEmpty(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        // 4. COMPARE
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await _next(context);
    }
}