using System.Text.Json.Serialization;

namespace SteamHeartAPI.Models
{
    public class SteamUserStatsResponse
    {
        [JsonPropertyName("player_count")]
        public int PlayerCount { get; set; }

        [JsonPropertyName("result")]
        public int Success {get; set;}
    }

    

    
}