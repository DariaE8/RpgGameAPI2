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
    [ServiceFilter(typeof(LoggingActionFilter))] // логг
    public class GameLocationsController : ControllerBase
    {
        private readonly IGameLocationService _locationService;
        private readonly ILogger<GameLocationsController> _logger;

        public GameLocationsController(
            IGameLocationService locationService,
            ILogger<GameLocationsController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpGet("paged")]
        [SwaggerOperation(Summary = "Get locations with pagination", Description = "Retrieves locations with pagination, sorting and search")]
        [SwaggerResponse(200, "Successfully retrieved paged locations", typeof(PagedResult<GameLocationDto>))]
        public async Task<ActionResult<PagedResult<GameLocationDto>>> GetLocationsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting locations paged - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortOrder: {SortOrder}, Search: {Search}",
                page, pageSize, sortBy, sortOrder, search);

            var pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Search = search
            };

            var result = await _locationService.GetLocationsPagedAsync(pagination, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} locations for page {Page}", result.Items.Count(), page);
            return Ok(result);
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search locations", Description = "Searches locations by name, description or type")]
        [SwaggerResponse(200, "Successfully searched locations", typeof(List<GameLocationDto>))]
        public async Task<ActionResult<IEnumerable<GameLocationDto>>> SearchLocations(
            [FromQuery] string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                _logger.LogWarning("Search locations called with empty term");
                return BadRequest("Search term is required");
            }

            _logger.LogInformation("Searching locations with term: {Term}", term);
            
            var locations = await _locationService.SearchLocationsAsync(term, cancellationToken);
            
            _logger.LogInformation("Found {Count} locations for search term: {Term}", 
                locations.Count(), term);
            return Ok(locations);
        }

        [HttpGet("level-range")]
        [SwaggerOperation(Summary = "Get locations by level range", Description = "Retrieves locations within specified required level range")]
        [SwaggerResponse(200, "Successfully retrieved locations by level range", typeof(List<GameLocationDto>))]
        public async Task<ActionResult<IEnumerable<GameLocationDto>>> GetLocationsByLevelRange(
            [FromQuery] int minLevel = 1,
            [FromQuery] int maxLevel = 100,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);
            
            var locations = await _locationService.GetLocationsByLevelRangeAsync(minLevel, maxLevel, cancellationToken);
            
            _logger.LogInformation("Found {Count} locations in level range {MinLevel}-{MaxLevel}", 
                locations.Count(), minLevel, maxLevel);
            return Ok(locations);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get location by ID", Description = "Retrieves a specific location by its unique identifier")]
        [SwaggerResponse(200, "Successfully retrieved location", typeof(GameLocationDto))]
        [SwaggerResponse(404, "Location not found")]
        public async Task<ActionResult<GameLocationDto>> GetLocation(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting location by ID: {LocationId}", id);
            
            var location = await _locationService.GetLocationByIdAsync(id, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved location {LocationName} (ID: {LocationId})",
                location?.Name, id);
            return Ok(location);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new location", Description = "Creates a new game location with the provided information")]
        [SwaggerResponse(201, "Location created successfully", typeof(GameLocationDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [ValidateModelState] // валидация
        public async Task<ActionResult<GameLocationDto>> CreateLocation(
            CreateGameLocationDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new location with name: {LocationName}", createDto.Name);
            
            var location = await _locationService.CreateLocationAsync(createDto, cancellationToken);
            
            _logger.LogInformation("Successfully created location {LocationName} with ID: {LocationId}", 
                location.Name, location.Id);
            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Fully update location", Description = "Updates all location properties")]
        [SwaggerResponse(200, "Location updated successfully", typeof(GameLocationDto))]
        [SwaggerResponse(404, "Location not found")]
        [ValidateModelState]
        public async Task<ActionResult<GameLocationDto>> UpdateLocation(
            Guid id,
            UpdateGameLocationDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating location with ID: {LocationId}", id);
            
            var location = await _locationService.UpdateLocationAsync(id, updateDto, cancellationToken);
            
            _logger.LogInformation("Successfully updated location {LocationName} (ID: {LocationId})",
                location?.Name, id);
            return Ok(location);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete location", Description = "Deletes a location by its unique identifier")]
        [SwaggerResponse(204, "Location deleted successfully")]
        [SwaggerResponse(404, "Location not found")]
        public async Task<IActionResult> DeleteLocation(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting location with ID: {LocationId}", id);
            
            var result = await _locationService.DeleteLocationAsync(id, cancellationToken);
            if (!result)
            {
                _logger.LogWarning("Location not found for deletion with ID: {LocationId}", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted location with ID: {LocationId}", id);
            return NoContent();
        }

        [HttpGet("accessible/{playerLevel}")]
        [SwaggerOperation(Summary = "Get accessible locations", Description = "Retrieves locations accessible to players of specified level")]
        [SwaggerResponse(200, "Successfully retrieved accessible locations", typeof(List<GameLocationDto>))]
        public async Task<ActionResult<IEnumerable<GameLocationDto>>> GetAccessibleLocations(
            int playerLevel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting accessible locations for player level: {PlayerLevel}", playerLevel);
            
            var locations = await _locationService.GetAccessibleLocationsAsync(playerLevel, cancellationToken);
            
            _logger.LogInformation("Found {Count} accessible locations for level {PlayerLevel}", 
                locations.Count(), playerLevel);
            return Ok(locations);
        }

        [HttpGet("type/{type}")]
        [SwaggerOperation(Summary = "Get locations by type", Description = "Retrieves all locations of a specific type")]
        [SwaggerResponse(200, "Successfully retrieved locations by type", typeof(List<GameLocationDto>))]
        public async Task<ActionResult<IEnumerable<GameLocationDto>>> GetLocationsByType(
            LocationType type,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations by type: {LocationType}", type);
            
            var locations = await _locationService.GetLocationsByTypeAsync(type, cancellationToken);
            
            _logger.LogInformation("Found {Count} locations of type: {LocationType}", locations.Count(), type);
            return Ok(locations);
        }

                [HttpGet("stats/player-distribution")]
        [SwaggerOperation(Summary = "Get players distribution by location", Description = "Returns count of players in each location")]
        [SwaggerResponse(200, "Successfully retrieved player distribution", typeof(Dictionary<string, int>))]
        public async Task<ActionResult<Dictionary<string, int>>> GetPlayerDistribution(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players distribution by location");
            
            var distribution = await _locationService.GetPlayerDistributionAsync(cancellationToken);
            
            _logger.LogInformation("Player distribution by location: {@Distribution}", distribution);
            return Ok(distribution);
        }
    }
}