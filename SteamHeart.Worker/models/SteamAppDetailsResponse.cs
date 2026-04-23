using System.Text.Json.Serialization;

namespace SteamHeartAPI.Models
{
    public class SteamAppDetails
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamAppInfo? Data { get; set; }
    }

    public class SteamAppInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("steam_appid")]
        public int SteamAppId { get; set; }

        [JsonPropertyName("header_image")]
        public string? HeaderImage { get; set; }

        [JsonPropertyName("developers")]
        public List<string>? Developers { get; set; }

        [JsonPropertyName("publishers")]
        public List<string>? Publishers { get; set; }

        [JsonPropertyName("genres")]
        public List<SteamGenre>? Genres { get; set; }

        [JsonPropertyName("release_date")]
        public SteamReleaseDate? ReleaseDate { get; set; }
    }

    public class SteamGenre
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class SteamReleaseDate
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }
    }

    public class SteamSpyAppData
    {
        [JsonPropertyName("tags")]
        public Dictionary<string, int>? Tags { get; set; }
    }
}
