using System.Text.Json.Serialization;

namespace SteamHeartAPI.Models
{
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

        [JsonPropertyName("discount_percent")]
        public int DiscountPercent { get; set; }


    }
}