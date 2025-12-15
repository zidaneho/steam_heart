using System.Text.Json.Serialization;

namespace SteamScout.Models
{
    // 1. The wrapper wrapper. 
    // Steam returns { "success": true, "data": { ... } }
    public class SteamAppResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamGameData Data { get; set; }
    }

    // 2. The actual game data you want
    public class SteamGameData
    {
        [JsonPropertyName("steam_appid")]
        public int SteamAppId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("header_image")]
        public string HeaderImage { get; set; }

        // BEWARE: Free games (like Dota 2) often return null for price_overview.
        // Always make this nullable (?) to prevent crashes.
        [JsonPropertyName("price_overview")]
        public SteamPriceOverview? PriceOverview { get; set; }
    }

    // 3. The nested price object
    public class SteamPriceOverview
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        // Steam returns price in cents (e.g., 999 cents = $9.99)
        [JsonPropertyName("final")]
        public int Final { get; set; }

        [JsonPropertyName("final_formatted")]
        public string FinalFormatted { get; set; }
    }
}