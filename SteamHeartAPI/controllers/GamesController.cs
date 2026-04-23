using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        // GET: api/games/unenriched
        [HttpGet("unenriched")]
        public async Task<ActionResult> GetUnenriched([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var query = context.GameTable.Where(g => g.Developer == null).OrderBy(g => g.Id);
            var total = await query.CountAsync();
            var games = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new { Data = games, Page = page, PageSize = pageSize, TotalCount = total });
        }

        // PUT: api/games/batch-update
        [HttpPut("batch-update")]
        public async Task<ActionResult> BatchUpdate([FromBody] List<Game> games)
        {
            var appIds = games.Select(g => g.AppId).ToList();
            var existing = await context.GameTable
                .Where(g => appIds.Contains(g.AppId))
                .ToListAsync();

            foreach (var record in existing)
            {
                var update = games.First(g => g.AppId == record.AppId);
                record.Developer = update.Developer;
                record.Publisher = update.Publisher;
                record.Genre = update.Genre;
                record.ReleaseDate = update.ReleaseDate;
                record.HeaderImageUrl = update.HeaderImageUrl;
                record.Tags = update.Tags;
            }

            await context.SaveChangesAsync();
            return Ok(new { Updated = existing.Count });
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
        public async Task<ActionResult> ImportMultipleGames([FromBody] List<Game> games)
        {
            if (games == null || games.Count == 0)
                return Ok(new { Added = 0 });

            var appIds = games.Select(g => g.AppId).ToArray();
            var names = games.Select(g => g.Name).ToArray();

            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""GameTable"" (""AppId"", ""Name"")
                SELECT * FROM unnest(@appIds, @names)
                ON CONFLICT (""AppId"") DO NOTHING",
                new Npgsql.NpgsqlParameter("appIds", appIds),
                new Npgsql.NpgsqlParameter("names", names)
            );

            return Ok(new { Added = games.Count });
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