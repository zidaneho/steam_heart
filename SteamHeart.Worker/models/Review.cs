using System.ComponentModel.DataAnnotations;

namespace SteamHeartAPI.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; } // Internal DB ID

        // --- Relationships ---
        public int GameId { get; set; }
        public Game? Game { get; set; }

        // --- The Review Itself ---
        public string RecommendationId { get; set; }
        public string ReviewText { get; set; }
        public string Language { get; set; }
        public bool VotedUp { get; set; }

        public DateTime TimestampCreated { get; set; }
        public DateTime TimestampUpdated { get; set; }

        public double WeightedVoteScore { get; set; }
        public int VotesUp { get; set; }
        public int VotesFunny { get; set; }
        public int CommentCount { get; set; }

        // --- The Context ---
        public bool SteamPurchase { get; set; }
        public bool ReceivedForFree { get; set; }
        public bool WrittenDuringEarlyAccess { get; set; }

        // Added to match your DTO
        public bool PrimarilySteamDeck { get; set; }

        public bool ChangedVote { get; set; }

        // --- The Author (Flattened) ---
        public string AuthorSteamId { get; set; }
        public int AuthorNumGamesOwned { get; set; }
        public int AuthorNumReviews { get; set; }

        public int AuthorPlaytimeForever { get; set; }
        public int AuthorPlaytimeLastTwoWeeks { get; set; }
        public int AuthorPlaytimeAtReview { get; set; }
        public DateTime AuthorLastPlayed { get; set; }

        //custom fields
        public int UpdatedCount {get; set;}

    }
}
