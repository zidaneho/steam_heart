using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamHeartAPI.Data;
using SteamHeartAPI.Helpers;
using SteamHeartAPI.Models;
using SteamHeartAPI.Services;

namespace SteamHeartAPI.Controllers
{
    // This controller handles all reviews for a SPECIFIC game
    [Route("api/games/{gameId}/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly SteamHeartContext context;
        private readonly ISteamService steamService;

        public ReviewsController(SteamHeartContext context, ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/games/5/reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews(int gameId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // Simple check to ensure game exists
            if (!await context.GameTable.AnyAsync(g => g.Id == gameId))
            {
                return NotFound($"Game {gameId} not found.");
            }
            var query = context.ReviewTable.OrderBy(g => g.Id).Where(r => r.GameId == gameId);
            var total = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new {
                Data = reviews,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
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
                var response = await steamService.GetReviewStatsAsync(game.AppId, currentCursor);
                if (response == null || response.Reviews == null)
                    break;
                foreach (var review in response.Reviews)
                {
                    if (review.RecommendationId == null) continue;
                    
                    if (processedIds.Contains(review.RecommendationId))
                    {
                        moreDataAvailable = false;
                        break;
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
        public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = context.ReviewTable.OrderBy(g => g.Id);
            var total = await query.CountAsync();
            var games = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(new {
                Data = games,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
          });
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ReviewSummary[]>> GetReviewSummary(int gameId)
        {
            if (!await context.GameTable.AnyAsync(g => g.Id == gameId))
                return NotFound($"Game {gameId} not found.");
            var query = await context.ReviewTable
                .Where(r => r.GameId == gameId)
                .GroupBy(r => r.TimestampCreated.Date)
                .Select(g => new ReviewSummary
                {
                    Day = g.Key,
                    PositiveCount = g.Count(r => r.VotedUp),
                    NegativeCount = g.Count(r => !r.VotedUp)
                })
                .OrderBy(r => r.Day)
                .ToListAsync();
            return Ok(query);

        }
    }

}