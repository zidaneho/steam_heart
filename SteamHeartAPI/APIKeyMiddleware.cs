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

        // 1. FAIL SAFE: Block everything if server config is missing
        if (string.IsNullOrEmpty(apiKey))
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ApiKeyMiddleware>>();
            logger.LogError("CRITICAL: X_API_KEY is missing. Blocking request.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Server config error.");
            return;
        }

        string? extractedApiKey = null;
        bool keyCameFromQuery = false;

        // 2. SEARCH STRATEGY: Header -> Query -> Cookie

        // A. Check Header (Best for Postman/Code)
        if (context.Request.Headers.TryGetValue(APIKEYNAME, out var headerVal))
        {
            extractedApiKey = headerVal.ToString();
        }
        // B. Check Query String (Best for Browser Initial Visit)
        else if (context.Request.Query.TryGetValue(APIKEYNAME, out var queryVal))
        {
            extractedApiKey = queryVal.ToString();
            keyCameFromQuery = true; // Mark this so we can set a cookie later
        }
        // C. Check Cookie (Best for Browser Assets/Subsequent requests)
        else if (context.Request.Cookies.TryGetValue(APIKEYNAME, out var cookieVal))
        {
            extractedApiKey = cookieVal;
        }

        // 3. IF MISSING EVERYWHERE -> 401
        if (string.IsNullOrEmpty(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        // 4. VALIDATE KEY
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        // 5. SUCCESS: If the user provided the key in the URL, save it to a Cookie.
        // This allows the browser to load scalar.js, styles.css, and openapi.json automatically.
        if (keyCameFromQuery)
        {
            context.Response.Cookies.Append(APIKEYNAME, extractedApiKey, new CookieOptions
            {
                HttpOnly = true, // Javascript can't steal it
                Secure = true,   // HTTPS only
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1) // Cookie lasts 1 hour
            });
        }

        await _next(context);
    }
}