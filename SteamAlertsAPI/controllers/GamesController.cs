using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Models;
using SteamAlertsAPI.Services;

namespace SteamAlertsAPI.Controllers
{
    [Route("api/games")] // Explicit lowercase is cleaner
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly SteamAlertsContext context;
        private readonly ISteamService steamService;

        public GamesController(SteamAlertsContext context, ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetAll()
        {
            return await context.GameTable.ToListAsync();
        }

        // GET: api/games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetById(int id)
        {
            var game = await context.GameTable.FindAsync(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        // POST: api/games (Import single game)
        [HttpPost]
        public async Task<ActionResult<Game>> ImportGame([FromBody] int appid)
        {
            var result = await TryImportGameLogic(appid);
            if (result.error != null)
            {
                if (result.error.Contains("exists")) return Conflict(result.error);
                return NotFound(result.error);
            }

            context.GameTable.Add(result.game);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = result.game.Id }, result.game);
        }

        // POST: api/games/batch (Import multiple)
        [HttpPost("batch")]
        public async Task<ActionResult<List<Game>>> ImportMultipleGames([FromBody] List<int> appids)
        {
            List<Game> newGames = new List<Game>();
            List<string> errors = new List<string>();

            foreach (var appid in appids)
            {
                var result = await TryImportGameLogic(appid);
                if (result.game != null) newGames.Add(result.game);
                else errors.Add($"Failed {appid}: {result.error}");
            }

            if (newGames.Any())
            {
                context.GameTable.AddRange(newGames);
                await context.SaveChangesAsync();
            }

            return Ok(new { Added = newGames, Errors = errors });
        }

        private async Task<(Game? game, string? error)> TryImportGameLogic(int appid)
        {
            var existingGame = await context.GameTable.FirstOrDefaultAsync(g => g.AppId == appid);
            if (existingGame != null) return (null, $"Game {appid} already exists.");

            var steamData = await steamService.GetGameDetailsAsync(appid);
            if (steamData == null) return (null, $"AppId {appid} not found on Steam.");

            var newGame = new Game { Name = steamData.Name, AppId = steamData.SteamAppId };
            return (newGame, null);
        }
    }
}