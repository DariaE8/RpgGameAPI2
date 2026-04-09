using Microsoft.AspNetCore.Mvc;
using RpgGame.Core.DTOs;
using RpgGame.Core.Interfaces;
using RpgGame.Core.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using RpgGame.API.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace RpgGame.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(LoggingActionFilter))]
    public class EnemiesController : ControllerBase
    {
        private readonly IEnemyService _enemyService;
        private readonly ILogger<EnemiesController> _logger;

        public EnemiesController(
            IEnemyService enemyService,
            ILogger<EnemiesController> logger)
        {
            _enemyService = enemyService;
            _logger = logger;
        }

        [HttpGet("paged")]
        [SwaggerOperation(Summary = "Get enemies with pagination", Description = "Retrieves enemies with pagination, sorting and search")]
        [SwaggerResponse(200, "Successfully retrieved paged enemies", typeof(PagedResult<EnemyDto>))]
        public async Task<ActionResult<PagedResult<EnemyDto>>> GetEnemiesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting enemies paged - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortOrder: {SortOrder}, Search: {Search}",
                page, pageSize, sortBy, sortOrder, search);

            var pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Search = search
            };

            var result = await _enemyService.GetEnemiesPagedAsync(pagination, cancellationToken);

            _logger.LogInformation("Retrieved {Count} enemies for page {Page}", result.Items.Count(), page);
            return Ok(result);
        }


        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search enemies", Description = "Searches enemies by name, location or type")]
        [SwaggerResponse(200, "Successfully searched enemies", typeof(List<EnemyDto>))]
        public async Task<ActionResult<IEnumerable<EnemyDto>>> SearchEnemies(
            [FromQuery] string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                _logger.LogWarning("Search enemies called with empty term");
                return BadRequest("Search term is required");
            }

            _logger.LogInformation("Searching enemies with term: {Term}", term);

            var enemies = await _enemyService.SearchEnemiesAsync(term, cancellationToken);

            _logger.LogInformation("Found {Count} enemies for search term: {Term}", enemies.Count(), term);
            return Ok(enemies);
        }


        [HttpGet("level-range")]
        [SwaggerOperation(Summary = "Get enemies by level range", Description = "Retrieves enemies within specified level range")]
        [SwaggerResponse(200, "Successfully retrieved enemies by level range", typeof(List<EnemyDto>))]
        public async Task<ActionResult<IEnumerable<EnemyDto>>> GetEnemiesByLevelRange(
            [FromQuery] int minLevel = 1,
            [FromQuery] int maxLevel = 100,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);

            var enemies = await _enemyService.GetEnemiesByLevelRangeAsync(minLevel, maxLevel, cancellationToken);

            _logger.LogInformation("Found {Count} enemies in level range {MinLevel}-{MaxLevel}",
                enemies.Count(), minLevel, maxLevel);
            return Ok(enemies);
        }



        [HttpGet("reward-range")]
        [SwaggerOperation(Summary = "Get enemies by reward range", Description = "Retrieves enemies within specified experience and gold reward range")]
        [SwaggerResponse(200, "Successfully retrieved enemies by reward range", typeof(List<EnemyDto>))]
        public async Task<ActionResult<IEnumerable<EnemyDto>>> GetEnemiesByRewardRange(
            [FromQuery] int minExp = 0,
            [FromQuery] int maxExp = 10000,
            [FromQuery] int minGold = 0,
            [FromQuery] int maxGold = 10000,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting enemies by reward range - Exp: {MinExp}-{MaxExp}, Gold: {MinGold}-{MaxGold}",
                minExp, maxExp, minGold, maxGold);

            var enemies = await _enemyService.GetEnemiesByRewardRangeAsync(
                minExp, maxExp, minGold, maxGold, cancellationToken);

            _logger.LogInformation("Found {Count} enemies in specified reward range", enemies.Count());
            return Ok(enemies);
        }



        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get enemy by ID", Description = "Retrieves a specific enemy by their unique identifier")]
        [SwaggerResponse(200, "Successfully retrieved enemy", typeof(EnemyDto))]
        [SwaggerResponse(404, "Enemy not found")]
        public async Task<ActionResult<EnemyDto>> GetEnemy(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemy by ID: {EnemyId}", id);

            try
            {
                var enemy = await _enemyService.GetEnemyByIdAsync(id, cancellationToken);
                _logger.LogInformation("Successfully retrieved enemy {EnemyName} (ID: {EnemyId})",
                    enemy!.Name, id);
                return Ok(enemy);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Enemy not found with ID: {EnemyId}", id);
                return NotFound($"Enemy with id {id} not found");
            }
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Create a new enemy", Description = "Creates a new enemy with the provided information")]
        [SwaggerResponse(201, "Enemy created successfully", typeof(EnemyDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [ValidateModelState]
        public async Task<ActionResult<EnemyDto>> CreateEnemy(
            [FromBody] CreateEnemyDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new enemy with name: {EnemyName}", createDto.Name);

            try
            {
                var enemy = await _enemyService.CreateEnemyAsync(createDto, cancellationToken);

                _logger.LogInformation("Successfully created enemy {EnemyName} with ID: {EnemyId}",
                    enemy.Name, enemy.Id);
                return CreatedAtAction(nameof(GetEnemy), new { id = enemy.Id }, enemy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input data for enemy creation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enemy");
                return StatusCode(500, "Internal server error");
            }
        }

        // ---------------------------
        // UPDATE
        // ---------------------------

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Fully update enemy", Description = "Updates all enemy properties")]
        [SwaggerResponse(200, "Enemy updated successfully", typeof(EnemyDto))]
        [SwaggerResponse(404, "Enemy not found")]
        public async Task<ActionResult<EnemyDto>> UpdateEnemy(
            Guid id,
            [FromBody] UpdateEnemyDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating enemy with ID: {EnemyId}", id);

            try
            {
                var enemy = await _enemyService.UpdateEnemyAsync(id, updateDto, cancellationToken);

                _logger.LogInformation("Successfully updated enemy {EnemyName} (ID: {EnemyId})",
                    enemy?.Name, id);
                return Ok(enemy);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Enemy not found for update with ID: {EnemyId}", id);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input data for enemy update");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enemy");
                return StatusCode(500, "Internal server error");
            }
        }

        // ---------------------------
        // DELETE
        // ---------------------------

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete enemy", Description = "Deletes an enemy by their unique identifier")]
        [SwaggerResponse(204, "Enemy deleted successfully")]
        [SwaggerResponse(404, "Enemy not found")]
        public async Task<IActionResult> DeleteEnemy(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting enemy with ID: {EnemyId}", id);

            try
            {
                var result = await _enemyService.DeleteEnemyAsync(id, cancellationToken);

                if (!result)
                {
                    _logger.LogWarning("Enemy not found for deletion with ID: {EnemyId}", id);
                    return NotFound($"Enemy with id {id} not found");
                }

                _logger.LogInformation("Successfully deleted enemy with ID: {EnemyId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting enemy with ID: {EnemyId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // ---------------------------
        // GET BY LOCATION
        // ---------------------------

        [HttpGet("location/{location}")]
        [SwaggerOperation(Summary = "Get enemies by location", Description = "Retrieves all enemies in a specific location")]
        [SwaggerResponse(200, "Successfully retrieved enemies", typeof(List<EnemyDto>))]
        public async Task<ActionResult<IEnumerable<EnemyDto>>> GetEnemiesByLocation(
            string location,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by location: {Location}", location);

            var enemies = await _enemyService.GetEnemiesByLocationAsync(location, cancellationToken);

            _logger.LogInformation("Found {Count} enemies in location: {Location}", enemies.Count(), location);
            return Ok(enemies);
        }

        // ---------------------------
        // GET BY TYPE
        // ---------------------------

        [HttpGet("type/{type:int}")]
        [SwaggerOperation(Summary = "Get enemies by type", Description = "Retrieves all enemies of a specific type")]
        [SwaggerResponse(200, "Successfully retrieved enemies by type", typeof(List<EnemyDto>))]
        public async Task<ActionResult<IEnumerable<EnemyDto>>> GetEnemiesByType(
            EnemyType type,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by type: {EnemyType}", type);

            var enemies = await _enemyService.GetEnemiesByTypeAsync(type, cancellationToken);

            _logger.LogInformation("Found {Count} enemies of type: {EnemyType}", enemies.Count(), type);
            return Ok(enemies);
        }

        // ---------------------------
        // STATS
        // ---------------------------

        [HttpGet("stats/count-by-type")]
        [SwaggerOperation(Summary = "Get enemies count by type", Description = "Returns count of enemies grouped by type")]
        [SwaggerResponse(200, "Successfully retrieved enemy counts by type", typeof(Dictionary<string, int>))]
        public async Task<ActionResult<Dictionary<string, int>>> GetEnemiesCountByType(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies count by type");

            var counts = await _enemyService.GetEnemiesCountByTypeAsync(cancellationToken);

            _logger.LogInformation("Retrieved enemy counts by type: {@Counts}", counts);
            return Ok(counts);
        }

        [HttpGet("stats/average-level")]
        [SwaggerOperation(Summary = "Get average enemy level", Description = "Returns the average level of all enemies")]
        [SwaggerResponse(200, "Successfully retrieved average enemy level", typeof(double))]
        public async Task<ActionResult<double>> GetAverageEnemyLevel(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting average enemy level");

            var averageLevel = await _enemyService.GetAverageEnemyLevelAsync(cancellationToken);

            _logger.LogInformation("Average enemy level: {AverageLevel}", averageLevel);
            return Ok(averageLevel);
        }

        [HttpGet("stats/total-gold-reward")]
        [SwaggerOperation(Summary = "Get total gold reward from all enemies", Description = "Returns the sum of gold rewards from all enemies")]
        [SwaggerResponse(200, "Successfully retrieved total gold reward", typeof(int))]
        public async Task<ActionResult<int>> GetTotalGoldReward(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting total gold reward from enemies");

            var totalGold = await _enemyService.GetTotalGoldRewardAsync(cancellationToken);

            _logger.LogInformation("Total gold reward from enemies: {TotalGold}", totalGold);
            return Ok(totalGold);
        }
    }
}
