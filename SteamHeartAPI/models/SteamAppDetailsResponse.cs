using System.Text.Json.Serialization;

namespace SteamHeartAPI.Models
{
    public class SteamAppDetails
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamAppInfo Data { get; set; }
    }

    public class SteamAppInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("steam_appid")]
        public int SteamAppId { get; set; }

        [JsonPropertyName("required_age")]
        public int RequiredAge { get; set; }

        [JsonPropertyName("is_free")]
        public bool IsFree { get; set; }

        [JsonPropertyName("dlc")]
        public List<int>? Dlc { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("detailed_description")]
        public string DetailedDescription { get; set; }

        [JsonPropertyName("about_the_game")]
        public string AboutTheGame { get; set; }

        [JsonPropertyName("supported_languages")]
        public string SupportedLanguages { get; set; }

        [JsonPropertyName("header_image")]
        public string HeaderImage { get; set; }

        [JsonPropertyName("capsule_image")]
        public string CapsuleImage { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("developers")]
        public List<string>? Developers { get; set; }

        [JsonPropertyName("publishers")]
        public List<string>? Publishers { get; set; }

        // Nullable: free games don't have price_overview
        [JsonPropertyName("price_overview")]
        public SteamPriceOverview? PriceOverview { get; set; }

        [JsonPropertyName("platforms")]
        public SteamPlatforms? Platforms { get; set; }

        [JsonPropertyName("categories")]
        public List<SteamCategory>? Categories { get; set; }

        [JsonPropertyName("genres")]
        public List<SteamGenre>? Genres { get; set; }

        [JsonPropertyName("recommendations")]
        public SteamRecommendations? Recommendations { get; set; }

        [JsonPropertyName("achievements")]
        public SteamAchievementsSummary? Achievements { get; set; }

        [JsonPropertyName("release_date")]
        public SteamReleaseDate? ReleaseDate { get; set; }
    }

    public class SteamGenre
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class SteamCategory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class SteamPlatforms
    {
        [JsonPropertyName("windows")]
        public bool Windows { get; set; }

        [JsonPropertyName("mac")]
        public bool Mac { get; set; }

        [JsonPropertyName("linux")]
        public bool Linux { get; set; }
    }

    public class SteamRecommendations
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class SteamAchievementsSummary
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class SteamReleaseDate
    {
        [JsonPropertyName("coming_soon")]
        public bool ComingSoon { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
