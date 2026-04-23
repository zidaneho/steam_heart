using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SteamHeartAPI.Models;

public class SteamCatalogService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey;

    private readonly string apiUrl;

    public SteamCatalogService()
    {
        _httpClient = new HttpClient();
        apiKey = Environment.GetEnvironmentVariable("X_Api_Key")
            ?? throw new Exception("X_Api_Key is missing!");
        apiUrl = Environment.GetEnvironmentVariable("API_URL")
            ?? throw new Exception("API_URL is missing!");
    }

    public async Task UpdateGameListAsync()
    {
        Console.WriteLine("--- Starting Catalog Update ---");
        Console.WriteLine("Warming up API...");
        try { await _httpClient.GetAsync($"{apiUrl}/api/games?pageSize=1"); } catch { }
        await Task.Delay(3000);

        // 1. Fetch the raw JSON from the repo you found
        var url = "https://raw.githubusercontent.com/jsnli/steamappidlist/master/data/games_appid.json";
        var steamGames = await _httpClient.GetFromJsonAsync<List<GitHubGameEntry>>(url);

        if (steamGames == null) return;
        Console.WriteLine($"Downloaded {steamGames.Count} games from GitHub.");
        var nullNameCount = steamGames.Count(g => string.IsNullOrWhiteSpace(g.Name));
        Console.WriteLine($"Games with null/empty names (will be skipped): {nullNameCount}");
        Game[] games = steamGames
            .Where(g => !string.IsNullOrWhiteSpace(g.Name))
            .Select(g => new Game { AppId = g.AppId, Name = g.Name })
            .ToArray();
        var batches = games.Chunk(100);
        int batchCount = 0;
        int totalBatches = batches.Count();

        foreach (var batch in batches)
        {
            batchCount++;
            bool success = false;
            int attempts = 0;
            while (!success && attempts < 3)
            {
                attempts++;
                success = await UploadGamesAsync(batch);
                if (!success)
                {
                    Console.WriteLine($"Batch {batchCount}/{totalBatches} failed (attempt {attempts}/3), retrying in 5s...");
                    await Task.Delay(5000);
                }
            }
            if (!success)
                Console.WriteLine($"Batch {batchCount}/{totalBatches} permanently failed — AppIds: {string.Join(", ", batch.Select(g => g.AppId))}");
            else
                Console.WriteLine($"Uploaded batch {batchCount}/{totalBatches}");

            await Task.Delay(200);
        }

        Console.WriteLine("--- Catalog Update Complete ---");
    }

    public async Task<bool> UploadGamesAsync(Game[] games)
    {
        var client = new HttpClient();
        var url = $"{apiUrl}/api/games/batch";

        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.PostAsJsonAsync(url, games);

        if (response.IsSuccessStatusCode)
            return true;

        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {response.StatusCode} - {error}");
        return false;
    }
}

// Helper class to match the JSON structure exactly
public class GitHubGameEntry
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("last_modified")]
    public long LastModified { get; set; }

    [JsonPropertyName("price_change_number")]
    public long PriceChangeNumber { get; set; }
}

public class BatchPayload
{
    public int[] appids { get; set; }
}