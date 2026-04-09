using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RpgGame.Core.DTOs;
using RpgGame.Core.Models;
using RpgGame.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using RpgGame.Core.Interfaces;
using RpgGame.Core.Exceptions;

namespace RpgGame.Services.Services
{
    public class QuestService : IQuestService
    {
        private readonly GameDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;

        public QuestService(
            GameDbContext context,
            IMapper mapper,
            ILogger<QuestService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuestDto?> GetQuestByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quest by ID: {QuestId}", id);
            
            var quest = await _context.Quests
                .AsSplitQuery()
                .Include(q => q.GameLocation)
                .Include(q => q.PlayersCompleted)
                .Include(q => q.RequiredEnemies)
                .Include(q => q.RequiredItems)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
                
            if (quest == null)
            {
                _logger.LogWarning("Quest not found with ID: {QuestId}", id);
                throw new NotFoundException($"Quest with ID {id} not found");
            }
            
            _logger.LogInformation("Successfully retrieved quest: '{QuestTitle}'", quest.Title);
            return _mapper.Map<QuestDto>(quest);
        }

        public async Task<PagedResult<QuestDto>> GetQuestsPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests paged - Page: {Page}, PageSize: {PageSize}", 
                pagination.Page, pagination.PageSize);
            
            var query = _context.Quests
                .Include(q => q.GameLocation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(q => 
                    q.Title.Contains(pagination.Search) ||
                    q.Description.Contains(pagination.Search) ||
                    q.Objective.Contains(pagination.Search)
                );
            }

            query = pagination.SortBy?.ToLower() switch
            {
                "title" => pagination.SortOrder == "desc" ? query.OrderByDescending(q => q.Title) : query.OrderBy(q => q.Title),
                "experiencereward" => pagination.SortOrder == "desc" ? query.OrderByDescending(q => q.ExperienceReward) : query.OrderBy(q => q.ExperienceReward),
                "status" => pagination.SortOrder == "desc" ? query.OrderByDescending(q => q.Status) : query.OrderBy(q => q.Status),
                "createdat" => pagination.SortOrder == "desc" ? query.OrderByDescending(q => q.CreatedAt) : query.OrderBy(q => q.CreatedAt),
                _ => query.OrderBy(q => q.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<QuestDto>
            {
                Items = _mapper.Map<List<QuestDto>>(items),
                PageNumber = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<QuestDto>> SearchQuestsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching quests with term: {SearchTerm}", searchTerm);
            
            var quests = await _context.Quests
                .Include(q => q.GameLocation)
                .AsNoTracking()
                .Where(q => 
                    q.Title.Contains(searchTerm) ||
                    q.Description.Contains(searchTerm) ||
                    q.Objective.Contains(searchTerm) ||
                    (q.GameLocation != null && q.GameLocation.Name.Contains(searchTerm)) 
                )
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} quests for search term: {SearchTerm}", quests.Count(), searchTerm);
            return _mapper.Map<IEnumerable<QuestDto>>(quests);
        }

        public async Task<IEnumerable<QuestDto>> GetQuestsByExperienceRangeAsync(int minExp, int maxExp, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests by experience range: {MinExp}-{MaxExp}", minExp, maxExp);
            
            var quests = await _context.Quests
                .Include(q => q.GameLocation)
                .Where(q => q.ExperienceReward >= minExp && q.ExperienceReward <= maxExp)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} quests in experience range {MinExp}-{MaxExp}", 
                quests.Count(), minExp, maxExp);
            return _mapper.Map<IEnumerable<QuestDto>>(quests);
        }

        public async Task<IEnumerable<QuestDto>> GetAvailableQuestsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all available quests");
            
            var quests = await _context.Quests
                .Include(q => q.GameLocation)
                .Where(q => q.Status == QuestStatus.Available)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} available quests", quests.Count());
            return _mapper.Map<IEnumerable<QuestDto>>(quests);
        }

        public async Task<IEnumerable<QuestDto>> GetQuestsByStatusAsync(QuestStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting quests by status: {QuestStatus}", status);
            
            var quests = await _context.Quests
                .Include(q => q.GameLocation)
                .Where(q => q.Status == status)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} quests with status: {QuestStatus}", quests.Count(), status);
            return _mapper.Map<IEnumerable<QuestDto>>(quests);
        }

        public async Task<IEnumerable<QuestDto>> GetCompletedQuestsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all completed quests");
            
            var quests = await _context.Quests
                .Include(q => q.GameLocation)
                .Where(q => q.CurrentCount >= q.TargetCount) // Используем выражение вместо свойства IsCompleted
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} completed quests", quests.Count());
            return _mapper.Map<IEnumerable<QuestDto>>(quests);
        }

        public async Task<QuestDto> CreateQuestAsync(CreateQuestDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new quest with title: '{QuestTitle}'", createDto.Title);
            
            try
            {
                var quest = _mapper.Map<Quest>(createDto);
                
                _context.Quests.Add(quest);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully created quest '{QuestTitle}' with ID: {QuestId}", 
                    quest.Title, quest.Id);
                return _mapper.Map<QuestDto>(quest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quest with title: '{QuestTitle}'", createDto.Title);
                throw;
            }
        }

        public async Task<QuestDto?> UpdateQuestAsync(Guid id, UpdateQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating quest with ID: {QuestId}", id);
            
            try
            {
                var quest = await _context.Quests
                    .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
                    
                if (quest == null)
                {
                    _logger.LogWarning("Quest not found for update with ID: {QuestId}", id);
                    throw new NotFoundException($"Quest with ID {id} not found");
                }

                if (updateDto.Title != null) quest.Title = updateDto.Title;
                if (updateDto.Description != null) quest.Description = updateDto.Description;
                if (updateDto.Objective != null) quest.Objective = updateDto.Objective;
                if (updateDto.TargetCount.HasValue) quest.TargetCount = updateDto.TargetCount.Value;
                if (updateDto.ExperienceReward.HasValue) quest.ExperienceReward = updateDto.ExperienceReward.Value;
                if (updateDto.GoldReward.HasValue) quest.GoldReward = updateDto.GoldReward.Value;
                if (updateDto.RequiredLocation != null) 
                {
                    var location = await _context.GameLocations
                        .FirstOrDefaultAsync(l => l.Name == updateDto.RequiredLocation, cancellationToken);
                    
                    if (location != null)
                    {
                        quest.GameLocation = location; 
                    }
                }

                quest.UpdateTimestamps();
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated quest '{QuestTitle}' (ID: {QuestId})", 
                    quest.Title, id);
                return _mapper.Map<QuestDto>(quest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quest with ID: {QuestId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteQuestAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting quest with ID: {QuestId}", id);
            
            var quest = await _context.Quests
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
                
            if (quest == null)
            {
                _logger.LogWarning("Quest not found for deletion with ID: {QuestId}", id);
                return false;
            }

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully deleted quest with ID: {QuestId}", id);
            return true;
        }

        public async Task<QuestDto?> UpdateQuestProgressAsync(Guid questId, int progress = 1, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating progress for quest {QuestId} by {Progress}", questId, progress);
            
            try
            {
                var quest = await _context.Quests
                    .FirstOrDefaultAsync(q => q.Id == questId, cancellationToken);
                    
                if (quest == null)
                {
                    _logger.LogWarning("Quest not found for progress update with ID: {QuestId}", questId);
                    throw new NotFoundException($"Quest with ID {questId} not found");
                }

                var originalStatus = quest.Status;
                var originalProgress = quest.Progress;
                quest.UpdateProgress(progress);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Updated progress for quest '{QuestTitle}'. Progress: {CurrentCount}/{TargetCount} ({Progress}%)",
                    quest.Title, quest.CurrentCount, quest.TargetCount, quest.Progress);
                    
                if (quest.Status != originalStatus)
                {
                    _logger.LogInformation("Quest '{QuestTitle}' status changed from {OriginalStatus} to {NewStatus}",
                        quest.Title, originalStatus, quest.Status);
                }
                
                return _mapper.Map<QuestDto>(quest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress for quest with ID: {QuestId}", questId);
                throw;
            }
        }

        // Агрегация: квесты по статусам
        public async Task<Dictionary<string, int>> GetQuestsCountByStatusAsync(CancellationToken cancellationToken = default)
        {
            var counts = await _context.Quests
                .AsNoTracking()
                .GroupBy(q => q.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
            
            return counts;
        }

        // Агрегация: общее вознаграждение за все квесты
        public async Task<QuestRewardsDto> GetTotalQuestRewardsAsync(CancellationToken cancellationToken = default)
        {
            var rewards = await _context.Quests
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new QuestRewardsDto
                {
                    TotalExperience = g.Sum(q => q.ExperienceReward),
                    TotalGold = g.Sum(q => q.GoldReward),
                    QuestCount = g.Count()
                })
                .FirstOrDefaultAsync(cancellationToken);
            
            return rewards ?? new QuestRewardsDto();
        }

    }
}
