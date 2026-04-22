using System.Text.Json.Serialization;

namespace SteamHeartAPI.Models
{
    public class SteamSpyAppData
    {
        [JsonPropertyName("tags")]
        public Dictionary<string, int> Tags { get; set; }
    }
}