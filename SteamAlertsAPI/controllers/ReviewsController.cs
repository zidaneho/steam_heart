using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Helpers;
using SteamAlertsAPI.Models;
using SteamAlertsAPI.Services;

namespace SteamAlertsAPI.Controllers
{
    // This controller handles all reviews for a SPECIFIC game
    [Route("api/games/{gameId}/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly SteamAlertsContext context;
        private readonly ISteamService steamService;

        public ReviewsController(SteamAlertsContext context, ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/games/5/reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews(int gameId)
        {
            // Simple check to ensure game exists
            if (!await context.GameTable.AnyAsync(g => g.Id == gameId))
            {
                return NotFound($"Game {gameId} not found.");
            }

            return await context.ReviewTable
                .Where(r => r.GameId == gameId)
                .OrderByDescending(r => r.TimestampCreated) // Good default sort
                .ToListAsync();
        }

        // POST: api/games/5/reviews/sync
        [HttpPost("sync")]
        public async Task<ActionResult> SyncReviews(int gameId)
        {
            var game = await context.GameTable.FindAsync(gameId);
            if (game == null) return NotFound($"Game {gameId} not found.");



            // Optimization: Load existing IDs into memory
            var existingReviewsDict = await context.ReviewTable
                .Where(r => r.GameId == gameId)
                .ToDictionaryAsync(r => r.RecommendationId);

            var processedIds = new HashSet<string>();

            string currentCursor = "*";
            bool moreDataAvailable = true;
            int totalSynced = 0;
            while (moreDataAvailable)
            {
                var response = await steamService.GetReviewStatsAsync(game.AppId);
                if (response == null || response.Reviews == null)
                    break;
                foreach (var review in response.Reviews)
                {
                    if (review.RecommendationId == null) continue;

                    if (processedIds.Contains(review.RecommendationId))
                    {
                        continue;
                    }
                    processedIds.Add(review.RecommendationId);

                    if (existingReviewsDict.TryGetValue(review.RecommendationId, out var existingReview))
                    {
                        // Update Logic
                        DateTime time = Utilities.UnixTimeStampToDateTime(review.TimestampUpdated);
                        if (time > existingReview.TimestampUpdated)
                        {
                            existingReview.TimestampUpdated = time;
                            existingReview.UpdatedCount++;
                            existingReview.ReviewText = review.ReviewText ?? "";
                            existingReview.VotedUp = review.VotedUp;
                            existingReview.VotesUp = review.VotesUp;
                            existingReview.VotesFunny = review.VotesFunny;
                            existingReview.CommentCount = review.CommentCount;

                            // Add other fields as needed...
                        }
                    }
                    else
                    {
                        // Insert Logic
                        Review r = new Review
                        {
                            GameId = gameId,
                            RecommendationId = review.RecommendationId,
                            ReviewText = review.ReviewText ?? "",
                            Language = review.Language ?? "unknown",
                            VotedUp = review.VotedUp,
                            VotesUp = review.VotesUp,
                            VotesFunny = review.VotesFunny,
                            CommentCount = review.CommentCount,
                            SteamPurchase = review.SteamPurchase,
                            WrittenDuringEarlyAccess = review.WrittenDuringEarlyAccess,
                            PrimarilySteamDeck = review.PrimarilySteamDeck,
                            TimestampCreated = Utilities.UnixTimeStampToDateTime(review.TimestampCreated),
                            TimestampUpdated = Utilities.UnixTimeStampToDateTime(review.TimestampUpdated),

                            // Handle Author nulls safely
                            AuthorSteamId = review.Author?.SteamId ?? "Unknown",
                            AuthorNumGamesOwned = review.Author?.NumGamesOwned ?? 0,
                            AuthorNumReviews = review.Author?.NumReviews ?? 0,
                            AuthorPlaytimeForever = review.Author?.PlaytimeForever ?? 0,
                            AuthorPlaytimeAtReview = review.Author?.PlaytimeAtReview ?? 0,
                            AuthorLastPlayed = Utilities.UnixTimeStampToDateTime(review.Author?.LastPlayedTimestamp ?? 0),

                            UpdatedCount = 0
                        };
                        context.ReviewTable.Add(r);
                    }
                }

                await context.SaveChangesAsync();
                totalSynced += response.Reviews.Count;
                if (response.Cursor == currentCursor)
                {
                    moreDataAvailable = false;
                }
                else
                {
                    currentCursor = response.Cursor;
                }
                await Task.Delay(2000);
            }


            return Ok(new { Count = totalSynced, Message = "Reviews synced successfully." });
        }
        [HttpGet("~/api/reviews")]
        public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews()
        {
            // Warning: This returns EVERYTHING. 
            // Recommended: Use .Take(100) to prevent crashing if you have 50k reviews.
            return await context.ReviewTable
                .OrderByDescending(r => r.TimestampCreated)
                .Take(100)
                .ToListAsync();
        }
    }

}