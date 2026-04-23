using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

public class SteamCsvExportService
{
    private readonly HttpClient _httpClient;

    public SteamCsvExportService()
    {
        _httpClient = new HttpClient();
    }

    public async Task ExportToCsvAsync(string outputPath)
    {
        Console.WriteLine("--- Starting CSV Export ---");

        var url = "https://raw.githubusercontent.com/jsnli/steamappidlist/master/data/games_appid.json";
        var steamGames = await _httpClient.GetFromJsonAsync<List<GitHubGameEntry>>(url);

        if (steamGames == null)
        {
            Console.WriteLine("Failed to download game list.");
            return;
        }

        Console.WriteLine($"Downloaded {steamGames.Count} games from GitHub.");

        var filtered = steamGames
            .Where(g => !string.IsNullOrWhiteSpace(g.Name))
            .ToList();

        Console.WriteLine($"Skipped {steamGames.Count - filtered.Count} games with null/empty names.");

        var sb = new StringBuilder();
        sb.AppendLine("AppId,Name");

        foreach (var game in filtered)
        {
            // Escape any quotes in the name and wrap in quotes to handle commas
            var safeName = game.Name.Replace("\"", "\"\"");
            sb.AppendLine($"{game.AppId},\"{safeName}\"");
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);

        Console.WriteLine($"Exported {filtered.Count} games to {outputPath}");
        Console.WriteLine("--- CSV Export Complete ---");
    }
}
