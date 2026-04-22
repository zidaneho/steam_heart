using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamHeartAPI.Data;
using SteamHeartAPI.Models;
using SteamHeartAPI.Services;

namespace SteamHeartAPI.Controllers
{
    [Route("api/games")] // Explicit lowercase is cleaner
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly SteamHeartContext context;
        private readonly ISteamService steamService;

        public GamesController(SteamHeartContext context, ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = context.GameTable.OrderBy(g => g.Id);
            var total = await query.CountAsync();
            var games = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return Ok(new
            {
                Data = games,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
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
        public async Task<ActionResult<List<Game>>> ImportMultipleGames([FromBody] List<Game> games)
        {
            //We will trust that the input is correct
            List<Game> newGames = new List<Game>();
            List<string> errors = new List<string>();

            foreach (var game in games)
            {
                var existingGame = await context.GameTable.FirstOrDefaultAsync(g => g.AppId == game.AppId);
                if (existingGame != null)
                {
                    continue;
                }
                if (game != null) newGames.Add(game);
                else errors.Add($"Failed {game.AppId}");
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

            var steamData = await steamService.GetAppInfoAsync(appid);
            if (steamData == null) return (null, $"AppId {appid} not found on Steam.");

            var tags = await steamService.GetTagsAsync(appid);

            var newGame = new Game { Name = steamData.Name, AppId = steamData.SteamAppId, Tags = tags, Developer = steamData.Developers?.FirstOrDefault() ?? "Unknown", Publisher = steamData.Publishers?.FirstOrDefault() ?? "Unknown", Genre = steamData.Genres?.FirstOrDefault()?.Description ?? "Unknown", ReleaseDate = steamData.ReleaseDate?.Date ?? "Unknown", HeaderImageUrl = steamData.HeaderImage ?? "Unknown" };
            return (newGame, null);
        }
    }
}