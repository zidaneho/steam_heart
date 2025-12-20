using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Data;
using SteamAlertsAPI.Models;
using SteamAlertsAPI.Services;

namespace SteamAlertsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly SteamAlertsContext context;
        private readonly ISteamService steamService;
        public GamesController(SteamAlertsContext context,ISteamService steamService)
        {
            this.context = context;
            this.steamService = steamService;
        }

        // GET: api/games
        [HttpGet]
       public async Task<ActionResult<IEnumerable<Game>>> GetAll()
        {
            // usage: _context.TableName.ToListAsync()
            return await context.GameTable.ToListAsync();
        }
        // GET: api/metrics
        [HttpGet("metrics")]
        public async Task<ActionResult<IEnumerable<Metric>>> GetAllMetrics()
        {
            return await context.MetricTable.ToListAsync();
        }
        // GET: api/games/1/metrics
        [HttpGet("{id}/metrics")]
        public async Task<ActionResult<IEnumerable<Metric>>> GetMetricsByAppID(int gameId)
        {
            var game = await context.GameTable.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }
            return await context.MetricTable.Where(m => m.GameId == gameId).OrderByDescending(m => m.Timestamp).ToListAsync();
        }

        // GET: api/games/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetById(int id)
        {
            var game = await context.GameTable.FindAsync(id);

            if (game == null)
            {
                return NotFound(); // Returns HTTP 404
            }

            return Ok(game); // Returns HTTP 200 with the specific game
        }
        // POST: api/games/import
        // The user sends some appid, we will save the game depending on the appid
        [HttpPost("import")]
        public async Task<ActionResult<Game>> ImportGame([FromBody] int appid)
        {
            var existingGame = await context.GameTable.FirstOrDefaultAsync(g => g.AppId == appid);
            if (existingGame != null)
            {
                return Conflict(new { message = $"Game with AppId {appid} already exists: {existingGame.Name}" });
            }
            var steamData = await steamService.GetGameDetailsAsync(appid);
            
            if (steamData == null)
            {
                return NotFound($"Could not find game with AppId {appid} on Steam.");
            }
            var newGame = new Game
            {
                Id = context.GameTable.Count() + 1,
                Name = steamData.Name,
                AppId = steamData.SteamAppId
            };
            context.GameTable.Add(newGame);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById),new {id=newGame.Id},newGame);

        }
        // POST: api/games/import
        // The user sends some appid, we will save the metric depending on the appid
        [HttpPost("import/metric")]
        public async Task<ActionResult<Game>> ImportMetric([FromBody] int appid)
        {
            var existingGame = await context.GameTable.FirstOrDefaultAsync(g => g.AppId == appid);
            if (existingGame == null)
            {
                return NotFound($"Could not find game with AppId {appid} on Steam.");
            }
            Metric metric = await steamService.GetMetricAsync(appid);
            
            metric.Game = existingGame;
            metric.GameId = existingGame.Id;

            context.MetricTable.Add(metric);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById),new {id=metric.Id},metric);

        }

       
    
    }
}