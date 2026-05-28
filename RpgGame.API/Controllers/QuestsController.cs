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
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _questService;
        private readonly ILogger<QuestsController> _logger;

        public QuestsController(
            IQuestService questService,
            ILogger<QuestsController> logger)
        {
            _questService = questService;
            _logger = logger;
        }


        [HttpGet("paged")]
        [SwaggerOperation(Summary = "Get quests with pagination", Description = "Retrieves quests with pagination, sorting and search")]
        [SwaggerResponse(200, "Successfully retrieved paged quests", typeof(PagedResult<QuestDto>))]
        public async Task<ActionResult<PagedResult<QuestDto>>> GetQuestsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Getting quests paged - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortOrder: {SortOrder}, Search: {Search}",
                page, pageSize, sortBy, sortOrder, search);

            var pagination = new PaginationDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Search = search
            };

            var result = await _questService.GetQuestsPagedAsync(pagination, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} quests for page {Page}", result.Items.Count(), page);
            return Ok(result);
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search quests", Description = "Searches quests by title, description or objective")]
        [SwaggerResponse(200, "Successfully searched quests", typeof(List<QuestDto>))]
        public async Task<ActionResult<IEnumerable<QuestDto>>> SearchQuests(
            [FromQuery] string term,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                _logger.LogWarning("Search quests called with empty term");
                return BadRequest("Search term is required");
            }

            _logger.LogInformation("Searching quests with term: {Term}", term);
            
            var quests = await _questService.SearchQuestsAsync(term, cancellationToken);
            
            _logger.LogInformation("Found {Count} quests for search term: {Term}", quests.Count(), term);
            return Ok(quests);
        }

        [HttpGet("experience-range")]
        [SwaggerOperation(Summary = "Get quests by experience range", Description = "Retrieves quests within specified experience reward range")]
        [SwaggerResponse(200, "Successfully retrieved quests by experience range", typeof(List<QuestDto>))]
        public async Task<ActionResult<IEnumerable<QuestDto>>> GetQuestsByExperienceRange(
            [FromQuery] int minExp = 0,
            [FromQuery] int maxExp = 10000,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests by experience range: {MinExp}-{MaxExp}", minExp, maxExp);
            
            var quests = await _questService.GetQuestsByExperienceRangeAsync(minExp, maxExp, cancellationToken);
            
            _logger.LogInformation("Found {Count} quests in experience range {MinExp}-{MaxExp}", 
                quests.Count(), minExp, maxExp);
            return Ok(quests);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get quest by ID", Description = "Retrieves a specific quest by its unique identifier")]
        [SwaggerResponse(200, "Successfully retrieved quest", typeof(QuestDto))]
        [SwaggerResponse(404, "Quest not found")]
        public async Task<ActionResult<QuestDto>> GetQuest(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quest by ID: {QuestId}", id);
            
            var quest = await _questService.GetQuestByIdAsync(id, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved quest '{QuestTitle}' (ID: {QuestId})",
                quest?.Title, id);
            return Ok(quest);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new quest", Description = "Creates a new quest with the provided information")]
        [SwaggerResponse(201, "Quest created successfully", typeof(QuestDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [ValidateModelState] // валидация
        public async Task<ActionResult<QuestDto>> CreateQuest(
            CreateQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new quest with title: '{QuestTitle}'", createDto.Title);
            
            var quest = await _questService.CreateQuestAsync(createDto, cancellationToken);
            
            _logger.LogInformation("Successfully created quest '{QuestTitle}' with ID: {QuestId}", 
                quest.Title, quest.Id);
            return CreatedAtAction(nameof(GetQuest), new { id = quest.Id }, quest);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Fully update quest", Description = "Updates all quest properties")]
        [SwaggerResponse(200, "Quest updated successfully", typeof(QuestDto))]
        [SwaggerResponse(404, "Quest not found")]
        [ValidateModelState]
        public async Task<ActionResult<QuestDto>> UpdateQuest(
            Guid id,
            UpdateQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating quest with ID: {QuestId}", id);
            
            var quest = await _questService.UpdateQuestAsync(id, updateDto, cancellationToken);
            
            _logger.LogInformation("Successfully updated quest '{QuestTitle}' (ID: {QuestId})",
                quest?.Title, id);
            return Ok(quest);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete quest", Description = "Deletes a quest by its unique identifier")]
        [SwaggerResponse(204, "Quest deleted successfully")]
        [SwaggerResponse(404, "Quest not found")]
        public async Task<IActionResult> DeleteQuest(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting quest with ID: {QuestId}", id);
            
            var result = await _questService.DeleteQuestAsync(id, cancellationToken);
            if (!result)
            {
                _logger.LogWarning("Quest not found for deletion with ID: {QuestId}", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted quest with ID: {QuestId}", id);
            return NoContent();
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(Summary = "Get quests by status", Description = "Retrieves all quests with the specified status")]
        [SwaggerResponse(200, "Successfully retrieved quests by status", typeof(List<QuestDto>))]
        public async Task<ActionResult<IEnumerable<QuestDto>>> GetQuestsByStatus(
            QuestStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests by status: {QuestStatus}", status);
            
            var quests = await _questService.GetQuestsByStatusAsync(status, cancellationToken);
            
            _logger.LogInformation("Found {Count} quests with status: {QuestStatus}", quests.Count(), status);
            return Ok(quests);
        }

                [HttpGet("stats/count-by-status")]
        [SwaggerOperation(Summary = "Get quests count by status", Description = "Returns count of quests grouped by status")]
        [SwaggerResponse(200, "Successfully retrieved quest counts by status", typeof(Dictionary<string, int>))]
        public async Task<ActionResult<Dictionary<string, int>>> GetQuestsCountByStatus(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests count by status");
            
            var counts = await _questService.GetQuestsCountByStatusAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved quest counts by status: {@Counts}", counts);
            return Ok(counts);
        }

        [HttpGet("stats/total-rewards")]
        [SwaggerOperation(Summary = "Get total rewards from all quests", Description = "Returns the sum of experience and gold rewards from all quests")]
        [SwaggerResponse(200, "Successfully retrieved total quest rewards", typeof(QuestRewardsDto))]
        public async Task<ActionResult<QuestRewardsDto>> GetTotalQuestRewards(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting total rewards from all quests");
            
            var rewards = await _questService.GetTotalQuestRewardsAsync(cancellationToken);
            
            _logger.LogInformation("Total quest rewards: {@Rewards}", rewards);
            return Ok(rewards);
        }
    }
}