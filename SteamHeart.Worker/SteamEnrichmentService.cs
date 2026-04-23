using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SteamHeartAPI.Models;

public class SteamEnrichmentService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    // Steam allows roughly 200 requests/min — 300ms delay keeps us safe
    private const int DelayBetweenRequestsMs = 300;

    public SteamEnrichmentService()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("X_Api_Key")
            ?? throw new Exception("X_Api_Key is missing!");
        _apiUrl = Environment.GetEnvironmentVariable("API_URL")
            ?? throw new Exception("API_URL is missing!");

        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }

    public async Task EnrichAllAsync()
    {
        Console.WriteLine("--- Starting Enrichment ---");

        int page = 1;
        int pageSize = 100;
        int totalEnriched = 0;
        int totalFailed = 0;

        while (true)
        {
            // 1. Fetch next page of unenriched games from the API
            var response = await _httpClient.GetFromJsonAsync<UnenrichedResponse>(
                $"{_apiUrl}/api/games/unenriched?page={page}&pageSize={pageSize}");

            if (response == null || response.Data.Count == 0)
            {
                Console.WriteLine("No more unenriched games found.");
                break;
            }

            Console.WriteLine($"Enriching page {page} ({response.Data.Count} games, {response.TotalCount} remaining total)...");

            var enriched = new List<Game>();

            // 2. Fetch details from Steam for each game
            foreach (var game in response.Data)
            {
                try
                {
                    var details = await FetchSteamDetailsAsync(game.AppId);
                    var tags = await FetchSteamSpyTagsAsync(game.AppId);

                    if (details != null)
                    {
                        game.Developer = details.Developers?.FirstOrDefault() ?? "Unknown";
                        game.Publisher = details.Publishers?.FirstOrDefault() ?? "Unknown";
                        game.Genre = details.Genres?.FirstOrDefault()?.Description ?? "Unknown";
                        game.ReleaseDate = details.ReleaseDate?.Date ?? "Unknown";
                        game.HeaderImageUrl = details.HeaderImage ?? "Unknown";
                        game.Tags = tags;
                        enriched.Add(game);
                    }
                    else
                    {
                        // Game not found on Steam — mark as enriched so we don't retry it
                        game.Developer = "N/A";
                        game.Publisher = "N/A";
                        game.Genre = "N/A";
                        game.ReleaseDate = "N/A";
                        game.HeaderImageUrl = "N/A";
                        enriched.Add(game);
                        totalFailed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error enriching AppId {game.AppId}: {ex.Message}");
                    totalFailed++;
                }

                await Task.Delay(DelayBetweenRequestsMs);
            }

            // 3. Send enriched batch back to the API
            if (enriched.Count > 0)
            {
                var updateResponse = await _httpClient.PutAsJsonAsync($"{_apiUrl}/api/games/batch-update", enriched);
                if (updateResponse.IsSuccessStatusCode)
                {
                    totalEnriched += enriched.Count;
                    Console.WriteLine($"  Saved {enriched.Count} enriched games. Total so far: {totalEnriched}");
                }
                else
                {
                    Console.WriteLine($"  Failed to save batch: {updateResponse.StatusCode}");
                }
            }

            // If we got fewer results than pageSize, we're on the last page
            if (response.Data.Count < pageSize)
                break;

            // Don't increment page — since we're updating records, page 1 will always
            // return the next batch of unenriched games
        }

        Console.WriteLine($"--- Enrichment Complete: {totalEnriched} enriched, {totalFailed} not found on Steam ---");
    }

    private async Task<SteamAppInfo?> FetchSteamDetailsAsync(int appId)
    {
        try
        {
            var url = $"https://store.steampowered.com/api/appdetails/?appids={appId}";
            var json = await _httpClient.GetStringAsync(url);
            var dict = JsonSerializer.Deserialize<Dictionary<string, SteamAppDetails>>(json);
            if (dict != null && dict.TryGetValue(appId.ToString(), out var details) && details.Success)
                return details.Data;
            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Dictionary<string, int>> FetchSteamSpyTagsAsync(int appId)
    {
        try
        {
            var url = $"https://steamspy.com/api.php?request=appdetails&appid={appId}";
            var data = await _httpClient.GetFromJsonAsync<SteamSpyAppData>(url);
            return data?.Tags ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }
}

public class UnenrichedResponse
{
    [JsonPropertyName("data")]
    public List<Game> Data { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}
