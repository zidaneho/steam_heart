using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Models;
using SteamScout.Models;

namespace SteamAlertsAPI.Services
{
    public interface ISteamService
    {
        Task<SteamStoreData> GetGameDetailsAsync(int appid);
        Task<Metric> GetMetricAsync(int appid);

        Task FetchAllMetricsAsync();

        Task<SteamUserReviewsResponse> GetReviewStatsAsync(int appid,string cursor = "*");


    }
    public class SteamService : ISteamService
    {
        private readonly HttpClient httpClient;
        private readonly SteamAlertsContext context;
        //api.steampowered.com<interface name>/<method name>/v1/?key=<api key>&format=<format>
        public SteamService(HttpClient httpClient, SteamAlertsContext context)
        {
            this.httpClient = httpClient;
            this.context = context;
        }

        public async Task<SteamStoreData> GetGameDetailsAsync(int appid)
        {
            string url = $"https://store.steampowered.com/api/appdetails/?appids={appid}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var dict = JsonSerializer.Deserialize<Dictionary<string,SteamStoreResponse>>(jsonString);
            if (dict != null && dict.ContainsKey(appid.ToString()))
            {
                var gameInfo = dict[appid.ToString()];
                if (gameInfo.Success)
                {
                    return gameInfo.Data;
                }
            }
            return null;
        }
        public async Task<int> GetPlayerCountAsync(int appid)
        {
            string url = $"https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid={appid}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var dict = JsonSerializer.Deserialize<Dictionary<string,SteamUserStatsResponse>>(jsonString);
            if (dict != null && dict.ContainsKey("response"))
            {
                var gameInfo = dict["response"];
                if (gameInfo.Success == 1)
                {
                    return gameInfo.PlayerCount;
                }
            }
            return -1;
        }
        public async Task<SteamUserReviewsResponse> GetReviewStatsAsync(int appid, string cursor = "*")
        {
            string encodedCursor = Uri.EscapeDataString(cursor);
            string url = $"https://store.steampowered.com/appreviews/{appid}?json=1&num_per_page=100&cursor={encodedCursor}";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var dict = JsonSerializer.Deserialize<SteamUserReviewsResponse>(jsonString);
           
            return dict;
            
        }
        
        public async Task<Metric> GetMetricAsync(int appid)
        {
            // 1. Start all tasks simultaneously
            var detailsTask = GetGameDetailsAsync(appid);
            var playerTask = GetPlayerCountAsync(appid);
            var reviewsTask = GetReviewStatsAsync(appid);

            // 2. Wait for all to complete
            await Task.WhenAll(detailsTask, playerTask, reviewsTask);

            // 3. Extract the results using await
            var details = await detailsTask;
            var playerCount = await playerTask;
            var reviews = await reviewsTask;

            // 4. Initialize object and map values with null-conditional checks
            return new Metric
            {
                // Use ?. to safely access PriceOverview if details is not null
                Price = details?.PriceOverview?.Final ?? 0,
                DiscountPercent = details?.PriceOverview?.DiscountPercent ?? 0,

                // PlayerCount is a value type (int) in your helper, so no null check needed
                PlayerCount = playerCount,

                // Map review data safely
                PositiveReviews = reviews?.QuerySummary?.TotalPositive ?? 0,
                NegativeReviews = reviews?.QuerySummary?.TotalNegative ?? 0,
                ReviewScoreLabel = reviews?.QuerySummary?.ReviewScoreDesc ?? "Unknown",

                Timestamp = DateTime.UtcNow
            };
        }

        public async Task FetchAllMetricsAsync()
        {
            var games = await context.GameTable.ToListAsync();
            foreach (var game in games)
            {
                var metric = await GetMetricAsync(game.AppId);
                if (metric != null)
                {
                    metric.Game = game;
                    metric.GameId = game.Id;
                    context.MetricTable.Add(metric);
                }

                await Task.Delay(1000);
            }
            await context.SaveChangesAsync();
        }

       
    }


}
