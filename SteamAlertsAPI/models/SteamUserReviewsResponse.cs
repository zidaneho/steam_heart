using System.Text.Json.Serialization;

namespace SteamAlertsAPI.Models
{
    public class SteamUserReviewsResponse
    {
        [JsonPropertyName("success")]
        public int Success { get; set; }

        [JsonPropertyName("query_summary")]
        public SteamReviewQuerySummary QuerySummary { get; set; }

        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }

        [JsonPropertyName("reviews")]
        public List<SteamUserReview> Reviews { get; set; }
    }

    public class SteamReviewQuerySummary
    {
        [JsonPropertyName("num_reviews")]
        public int NumReviews { get; set; }

        [JsonPropertyName("review_score")]
        public int ReviewScore { get; set; }

        [JsonPropertyName("review_score_desc")]
        public string ReviewScoreDesc { get; set; }

        [JsonPropertyName("total_positive")]
        public int TotalPositive { get; set; }

        [JsonPropertyName("total_negative")]
        public int TotalNegative { get; set; }

        [JsonPropertyName("total_reviews")]
        public int TotalReviews { get; set; }
    }
    public class SteamUserReview
    {
        [JsonPropertyName("recommendationid")]
        public string? RecommendationId { get; set; }

        [JsonPropertyName("author")]
        public SteamReviewAuthor? Author { get; set; }

        [JsonPropertyName("language")]
        public string? Language {get; set;}

        [JsonPropertyName("review")]
        public string? ReviewText { get; set; }

        [JsonPropertyName("voted_up")]
        public bool VotedUp { get; set; }

        [JsonPropertyName("votes_up")]
        public int VotesUp { get; set; }

        [JsonPropertyName("votes_funny")]
        public int VotesFunny {get; set;}

        [JsonPropertyName("timestamp_created")]
        public long TimestampCreated { get; set; }

        [JsonPropertyName("timestamp_updated")]
        public long TimestampUpdated {get; set;}

        [JsonPropertyName("comment_count")]
        public int CommentCount {get;set;}

        [JsonPropertyName("steam_purchase")]
        public bool SteamPurchase {get; set;}

        [JsonPropertyName("written_during_early_access")]
        public bool WrittenDuringEarlyAccess {get; set;}

        [JsonPropertyName("primarily_steam_deck")]
        public bool PrimarilySteamDeck {get; set;}

    }
    public class SteamReviewAuthor
    {
        [JsonPropertyName("steamid")]
        public string? SteamId { get; set; }

        [JsonPropertyName("num_games_owned")]
        public int NumGamesOwned { get; set; }

        [JsonPropertyName("num_reviews")]
        public int NumReviews {get; set;}

        [JsonPropertyName("playtime_forever")]
        public int PlaytimeForever { get; set; }

        [JsonPropertyName("playtime_last_two_weeks")]
        public int PlaytimeLastTwoWeeks {get; set;}

        [JsonPropertyName("playtime_at_review")]
        public int PlaytimeAtReview {get; set;}

        [JsonPropertyName("last_played")]
        public long LastPlayedTimestamp {get; set;}
    }
}