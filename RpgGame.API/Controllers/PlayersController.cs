using Microsoft.AspNetCore.Mvc;
using RpgGame.Core.DTOs;
using RpgGame.Core.Interfaces;
using RpgGame.Core.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using RpgGame.API.Filters;

namespace RpgGame.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(LoggingActionFilter))]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(
            IPlayerService playerService,
            ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [HttpGet("paged")]
        [SwaggerOperation(Summary = "Get players with pagination", Description = "Retrieves players with pagination, sorting and search")]
        [SwaggerResponse(200, "Successfully retrieved paged players", typeof(PagedResult<PlayerDto>))]
        public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayersPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting players paged - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortOrder: {SortOrder}, Search: {Search}",
                page, pageSize, sortBy, sortOrder, search);

            var pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Search = search
            };

            var result = await _playerService.GetPlayersPagedAsync(pagination, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} players for page {Page}", result.Items.Count(), page);
            return Ok(result);
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search players", Description = "Searches players by name, email or location")]
        [SwaggerResponse(200, "Successfully searched players", typeof(List<PlayerDto>))]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> SearchPlayers(
            [FromQuery] string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                _logger.LogWarning("Search players called with empty term");
                return BadRequest("Search term is required");
            }

            _logger.LogInformation("Searching players with term: {Term}", term);
            
            var players = await _playerService.SearchPlayersAsync(term, cancellationToken);
            
            _logger.LogInformation("Found {Count} players for search term: {Term}", players.Count(), term);
            return Ok(players);
        }

        [HttpGet("level-range")]
        [SwaggerOperation(Summary = "Get players by level range", Description = "Retrieves players within specified level range")]
        [SwaggerResponse(200, "Successfully retrieved players by level range", typeof(List<PlayerDto>))]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayersByLevelRange(
            [FromQuery] int minLevel = 1,
            [FromQuery] int maxLevel = 100,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);
            
            var players = await _playerService.GetPlayersByLevelRangeAsync(minLevel, maxLevel, cancellationToken);
            
            _logger.LogInformation("Found {Count} players in level range {MinLevel}-{MaxLevel}", 
                players.Count(), minLevel, maxLevel);
            return Ok(players);
        }

        [HttpGet("alive")]
        [SwaggerOperation(Summary = "Get alive players", Description = "Retrieves all players that are currently alive")]
        [SwaggerResponse(200, "Successfully retrieved alive players", typeof(List<PlayerDto>))]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetAlivePlayers(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all alive players");
            
            var players = await _playerService.GetAlivePlayersAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} alive players", players.Count());
            return Ok(players);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get player by ID", Description = "Retrieves a specific player by their unique identifier")]
        [SwaggerResponse(200, "Successfully retrieved player", typeof(PlayerDto))]
        [SwaggerResponse(404, "Player not found")]
        public async Task<ActionResult<PlayerDto>> GetPlayer(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting player by ID: {PlayerId}", id);
            
            var player = await _playerService.GetPlayerByIdAsync(id, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved player {PlayerName} (ID: {PlayerId})", 
                player.Name, id);
            return Ok(player);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new player", Description = "Creates a new player with the provided information")]
        [SwaggerResponse(201, "Player created successfully", typeof(PlayerDto))]
        [SwaggerResponse(400, "Invalid input data or email already exists")]
        [ValidateModelState]
        public async Task<ActionResult<PlayerDto>> CreatePlayer(
            CreatePlayerDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new player with name: {PlayerName}", createDto.Name);
            
            var player = await _playerService.CreatePlayerAsync(createDto, cancellationToken);
            
            _logger.LogInformation("Successfully created player {PlayerName} with ID: {PlayerId}", 
                player.Name, player.Id);
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Fully update player", Description = "Updates all player properties")]
        [SwaggerResponse(200, "Player updated successfully", typeof(PlayerDto))]
        [SwaggerResponse(404, "Player not found")]
        public async Task<ActionResult<PlayerDto>> UpdatePlayer(
            Guid id,
            UpdatePlayerDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating player with ID: {PlayerId}", id);
            
            var player = await _playerService.UpdatePlayerAsync(id, updateDto, cancellationToken);
            
            _logger.LogInformation("Successfully updated player {PlayerName} (ID: {PlayerId})", 
                player.Name, id);
            return Ok(player);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete player", Description = "Deletes a player by their unique identifier")]
        [SwaggerResponse(204, "Player deleted successfully")]
        [SwaggerResponse(404, "Player not found")]
        public async Task<IActionResult> DeletePlayer(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting player with ID: {PlayerId}", id);
            
            var result = await _playerService.DeletePlayerAsync(id, cancellationToken);
            if (!result)
            {
                _logger.LogWarning("Player not found for deletion with ID: {PlayerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted player with ID: {PlayerId}", id);
            return NoContent();
        }



                [HttpGet("stats/count-by-level")]
        [SwaggerOperation(Summary = "Get players count by level", Description = "Returns count of players grouped by level")]
        [SwaggerResponse(200, "Successfully retrieved players counts by level", typeof(Dictionary<int, int>))]
        public async Task<ActionResult<Dictionary<int, int>>> GetPlayersCountByLevel(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players count by level");
            
            var counts = await _playerService.GetPlayersCountByLevelAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved players counts by level: {@Counts}", counts);
            return Ok(counts);
        }

        [HttpGet("stats/total-gold")]
        [SwaggerOperation(Summary = "Get total gold from all players", Description = "Returns the sum of gold from all players")]
        [SwaggerResponse(200, "Successfully retrieved total player gold", typeof(int))]
        public async Task<ActionResult<int>> GetTotalPlayerGold(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting total gold from all players");
            
            var totalGold = await _playerService.GetTotalPlayerGoldAsync(cancellationToken);
            
            _logger.LogInformation("Total gold from players: {TotalGold}", totalGold);
            return Ok(totalGold);
        }

        [HttpGet("stats/overview")]
        [SwaggerOperation(Summary = "Get players statistics overview", Description = "Returns comprehensive statistics about players")]
        [SwaggerResponse(200, "Successfully retrieved player statistics", typeof(PlayerStatsDto))]
        public async Task<ActionResult<PlayerStatsDto>> GetPlayerStats(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players statistics overview");
            
            var stats = await _playerService.GetPlayerStatsAsync(cancellationToken);
            
            _logger.LogInformation("Player statistics: {@Stats}", stats);
            return Ok(stats);
        }

        [HttpPost("{playerId}/complete-quest-transaction/{questId}")]
        [SwaggerOperation(Summary = "Complete quest with transaction", Description = "Completes a quest for a player using transaction")]
        [SwaggerResponse(200, "Quest completed successfully with transaction", typeof(PlayerDto))]
        [SwaggerResponse(404, "Player or quest not found")]
        public async Task<ActionResult<PlayerDto>> CompleteQuestWithTransaction(
            Guid playerId,
            Guid questId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Completing quest {QuestId} for player {PlayerId} with transaction", questId, playerId);
            
            var player = await _playerService.CompleteQuestWithTransactionAsync(playerId, questId, cancellationToken);
            
            _logger.LogInformation("Player {PlayerName} successfully completed quest {QuestId} with transaction", 
                player.Name, questId);
            return Ok(player);
        }

    }
}