using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Models;
using SteamAlertsAPI.Services;

namespace SteamAlertsAPI.Controllers
{
    [Route("api/metrics")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly SteamAlertsContext context;
        private readonly ISteamService steamService;

        public MetricsController(SteamAlertsContext context, ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/metrics/500 (Get specific metric entry)
        [HttpGet("{id}")]
        public async Task<ActionResult<Metric>> GetById(int id)
        {
            var metric = await context.MetricTable.FindAsync(id);
            if (metric == null) return NotFound();
            return Ok(metric);
        }

        // GET: api/games/5/metrics (Get all metrics for a game)
        // The "~" overrides the controller-level route
        [HttpGet("~/api/games/{gameId}/metrics")]
        public async Task<ActionResult<IEnumerable<Metric>>> GetMetricsByGameID(int gameId)
        {
            var game = await context.GameTable.FindAsync(gameId);
            if (game == null) return NotFound($"Game {gameId} not found");

            return await context.MetricTable
                .Where(m => m.GameId == gameId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
        }

        // POST: api/metrics/import (Import based on AppId)
        [HttpPost("import")]
        public async Task<ActionResult<Game>> ImportMetric([FromBody] int appid)
        {
            var existingGame = await context.GameTable.FirstOrDefaultAsync(g => g.AppId == appid);
            if (existingGame == null) return NotFound($"Could not find game with AppId {appid} on Steam.");

            Metric metric = await steamService.GetMetricAsync(appid);

            metric.Game = existingGame;
            metric.GameId = existingGame.Id;

            context.MetricTable.Add(metric);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = metric.Id }, metric);
        }
        public async Task<ActionResult<Game>> ImportMetricByGameId([FromBody] int gameId)
        {
            var game = await context.GameTable.FindAsync(gameId);
            if (game == null) return NotFound($"Game {gameId} not found");

            return await ImportMetric(game.AppId);
        }
    }
}