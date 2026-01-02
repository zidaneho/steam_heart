namespace SteamAlertsAPI.Models
{
    public class Metric
    {
        //This is for data that does not change over time.
        public int Id { get; set; }

        public int GameId { get; set; }
        public Game? Game { get; set; }

        public int PlayerCount { get; set; }
        public decimal Price { get; set; }
        public int DiscountPercent { get; set; }

        public int PositiveReviews { get; set; }
        public int NegativeReviews { get; set; }
        public string? ReviewScoreLabel { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
