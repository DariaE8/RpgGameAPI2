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
    public class PlayerService : IPlayerService
    {
        private readonly GameDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(
            GameDbContext context,
            IMapper mapper,
            ILogger<PlayerService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlayerDto> GetPlayerByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting player by ID: {PlayerId}", id);
            
            var player = await _context.Players
                .AsSplitQuery()
                .Include(p => p.CompletedQuests)
                .Include(p => p.DefeatedEnemies)
                .Include(p => p.InventoryItems)
                .Include(p => p.CurrentGameLocation)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                
            if (player == null)
            {
                _logger.LogWarning("Player not found with ID: {PlayerId}", id);
                throw new NotFoundException($"Player with ID {id} not found");
            }
            
            _logger.LogInformation("Successfully retrieved player: {PlayerName}", player.Name);
            return _mapper.Map<PlayerDto>(player);
        }

        public async Task<PagedResult<PlayerDto>> GetPlayersPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players paged - Page: {Page}, PageSize: {PageSize}", 
                pagination.Page, pagination.PageSize);
            
            var query = _context.Players
            .Include(p => p.CurrentGameLocation)
            .AsNoTracking()
            .AsQueryable();
            
       

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => 
                    p.Name.Contains(pagination.Search) ||
                    p.Email.Contains(pagination.Search) ||
                    (p.CurrentGameLocation != null && p.CurrentGameLocation.Name.Contains(pagination.Search))
                    );
            }

            query = pagination.SortBy?.ToLower() switch
            {
                "name" => pagination.SortOrder == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "level" => pagination.SortOrder == "desc" ? query.OrderByDescending(p => p.Level) : query.OrderBy(p => p.Level),
                "experience" => pagination.SortOrder == "desc" ? query.OrderByDescending(p => p.Experience) : query.OrderBy(p => p.Experience),
                _ => query.OrderBy(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<PlayerDto>
            {
                Items = _mapper.Map<List<PlayerDto>>(items),
                PageNumber = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

            public async Task<IEnumerable<PlayerDto>> SearchPlayersAsync(string searchTerm, CancellationToken cancellationToken = default)
            {
                _logger.LogInformation("Searching players with term: {SearchTerm}", searchTerm);
                
                var players = await _context.Players
                .Include(p => p.CurrentGameLocation)
                .AsNoTracking()
                .Where(p => 
                    p.Name.Contains(searchTerm) ||
                    p.Email.Contains(searchTerm) ||
                    (p.CurrentGameLocation != null && p.CurrentGameLocation.Name.Contains(searchTerm)))
                .Select(p => new PlayerDto 
                {
                    Id = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Level = p.Level,
                    Experience = p.Experience,
                    Health = p.Health,
                    MaxHealth = p.MaxHealth,
                    Attack = p.Attack,
                    Gold = p.Gold,
                    CurrentLocation = p.CurrentGameLocation != null ? p.CurrentGameLocation.Name : "Unknown Location",
                    CompletedQuestsCount = p.CompletedQuests.Count,
                    InventoryItemsCount = p.InventoryItems.Count,
                    DefeatedEnemiesCount = p.DefeatedEnemies.Count,
                    IsAlive = p.IsAlive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(cancellationToken);
                
                _logger.LogInformation("Found {Count} players for search term: {SearchTerm}", players.Count, searchTerm);
                return players;
            }


        public async Task<IEnumerable<PlayerDto>> GetPlayersByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting players by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);
            
        var players = await _context.Players
                .Include(p => p.CurrentGameLocation)
                .AsNoTracking()
                .Where(p => p.Level >= minLevel && p.Level <= maxLevel)
                .Select(p => new PlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Level = p.Level,
                    Experience = p.Experience,
                    Health = p.Health,
                    MaxHealth = p.MaxHealth,
                    Attack = p.Attack,
                    Gold = p.Gold,
                    CurrentLocation = p.CurrentGameLocation != null ? p.CurrentGameLocation.Name : "Unknown Location",
                    CompletedQuestsCount = p.CompletedQuests.Count,
                    InventoryItemsCount = p.InventoryItems.Count,
                    DefeatedEnemiesCount = p.DefeatedEnemies.Count,
                    IsAlive = p.IsAlive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(cancellationToken);
                
            
            _logger.LogInformation("Found {Count} players in level range {MinLevel}-{MaxLevel}", 
                players.Count, minLevel, maxLevel);
            return players;
        }

        public async Task<IEnumerable<PlayerDto>> GetAlivePlayersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all alive players");
            
            var players = await _context.Players
                .Where(p => p.Health > 0)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} alive players", players.Count());
            return _mapper.Map<IEnumerable<PlayerDto>>(players);
        }

        public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new player with name: {PlayerName}, email: {PlayerEmail}", 
                createDto.Name, createDto.Email);
            
            try
            {
                var existingPlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.Email == createDto.Email, cancellationToken);
                if (existingPlayer != null)
                {
                    _logger.LogWarning("Player creation failed - email already exists: {PlayerEmail}", createDto.Email);
                    throw new ConflictException("Player with this email already exists");
                }

                var player = _mapper.Map<Player>(createDto);
                
                _context.Players.Add(player);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully created player {PlayerName} with ID: {PlayerId}", 
                    player.Name, player.Id);
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player with name: {PlayerName}", createDto.Name);
                throw;
            }
        }

        public async Task<PlayerDto> UpdatePlayerAsync(Guid id, UpdatePlayerDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating player with ID: {PlayerId}", id);
            
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for update with ID: {PlayerId}", id);
                    throw new NotFoundException($"Player with ID {id} not found");
                }

                if (updateDto.Name != null) player.Name = updateDto.Name;
                if (updateDto.Email != null) 
                {
                    var existingPlayer = await _context.Players
                        .FirstOrDefaultAsync(p => p.Email == updateDto.Email && p.Id != id, cancellationToken);
                    if (existingPlayer != null)
                    {
                        _logger.LogWarning("Player update failed - email already exists: {PlayerEmail}", updateDto.Email);
                        throw new ConflictException("Player with this email already exists");
                    }
                    player.Email = updateDto.Email;
                }
                if (updateDto.Level.HasValue) player.Level = updateDto.Level.Value;
                if (updateDto.Health.HasValue) player.Health = updateDto.Health.Value;
                if (updateDto.MaxHealth.HasValue) player.MaxHealth = updateDto.MaxHealth.Value;
                if (updateDto.Attack.HasValue) player.Attack = updateDto.Attack.Value;
                if (updateDto.Gold.HasValue) player.Gold = updateDto.Gold.Value;
 if (updateDto.CurrentLocation != null) 
{
    var location = await _context.GameLocations
        .FirstOrDefaultAsync(l => l.Name == updateDto.CurrentLocation, cancellationToken);
    
    if (location != null)
    {
        player.CurrentGameLocation = location; 
        player.LocationId = location.Id;       
    }
}
                player.UpdateTimestamps();
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated player {PlayerName} (ID: {PlayerId})", 
                    player.Name, id);
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player with ID: {PlayerId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting player with ID: {PlayerId}", id);
            
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                
            if (player == null)
            {
                _logger.LogWarning("Player not found for deletion with ID: {PlayerId}", id);
                return false;
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully deleted player with ID: {PlayerId}", id);
            return true;
        }

        public async Task<PlayerDto> AddExperienceAsync(Guid playerId, int experience, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding {Experience} experience to player with ID: {PlayerId}", experience, playerId);
            
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for adding experience with ID: {PlayerId}", playerId);
                    throw new NotFoundException($"Player with ID {playerId} not found");
                }

                var originalLevel = player.Level;
                player.AddExperience(experience);
                await _context.SaveChangesAsync(cancellationToken);
                
                if (player.Level > originalLevel)
                {
                    _logger.LogInformation("Player {PlayerName} leveled up from {OriginalLevel} to {NewLevel}", 
                        player.Name, originalLevel, player.Level);
                }
                
                _logger.LogInformation("Added {Experience} experience to player {PlayerName}. Total experience: {TotalExp}",
                    experience, player.Name, player.Experience);
                    
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding experience to player with ID: {PlayerId}", playerId);
                throw;
            }
        }

        public async Task<PlayerDto> HealPlayerAsync(Guid playerId, int amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Healing player with ID: {PlayerId} for {Amount} health", playerId, amount);
            
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for healing with ID: {PlayerId}", playerId);
                    throw new NotFoundException($"Player with ID {playerId} not found");
                }

                var originalHealth = player.Health;
                player.Heal(amount);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Healed player {PlayerName} for {Amount}. Health: {OriginalHealth} -> {CurrentHealth}",
                    player.Name, amount, originalHealth, player.Health);
                    
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error healing player with ID: {PlayerId}", playerId);
                throw;
            }
        }

        public async Task<PlayerDto> DamagePlayerAsync(Guid playerId, int damage, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Applying {Damage} damage to player with ID: {PlayerId}", damage, playerId);
            
            try
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for damage with ID: {PlayerId}", playerId);
                    throw new NotFoundException($"Player with ID {playerId} not found");
                }

                var originalHealth = player.Health;
                player.TakeDamage(damage);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Applied {Damage} damage to player {PlayerName}. Health: {OriginalHealth} -> {CurrentHealth}",
                    damage, player.Name, originalHealth, player.Health);
                    
                if (player.Health <= 0)
                {
                    _logger.LogWarning("Player {PlayerName} has been defeated", player.Name);
                }
                
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying damage to player with ID: {PlayerId}", playerId);
                throw;
            }
        }

        public async Task<PlayerDto> CompleteQuestAsync(Guid playerId, Guid questId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Completing quest {QuestId} for player {PlayerId}", questId, playerId);
            
            try
            {
                var player = await _context.Players
                    .Include(p => p.CompletedQuests)
                    .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for quest completion with ID: {PlayerId}", playerId);
                    throw new NotFoundException($"Player with ID {playerId} not found");
                }

                var quest = await _context.Quests
                    .FirstOrDefaultAsync(q => q.Id == questId, cancellationToken);
                    
                if (quest == null)
                {
                    _logger.LogWarning("Quest not found with ID: {QuestId}", questId);
                    throw new NotFoundException($"Quest with ID {questId} not found");
                }

                // Добавляем квест в список выполненных, если его там еще нет
                if (!player.CompletedQuests.Any(q => q.Id == questId))
                {
                    player.CompletedQuests.Add(quest);
                    player.Experience += quest.ExperienceReward;
                    player.Gold += quest.GoldReward;
                    player.UpdateTimestamps();
                    
                    await _context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Player {PlayerName} completed quest {QuestName} and gained {ExperienceReward} experience",
                    player.Name, quest.Title, quest.ExperienceReward);
                    
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing quest for player with ID: {PlayerId}", playerId);
                throw;
            }
        }

        public async Task<PlayerDto> DefeatEnemyAsync(Guid playerId, Guid enemyId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Player {PlayerId} defeating enemy {EnemyId}", playerId, enemyId);
            
            try
            {
                var player = await _context.Players
                    .Include(p => p.DefeatedEnemies)
                    .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
                    
                if (player == null)
                {
                    _logger.LogWarning("Player not found for enemy defeat with ID: {PlayerId}", playerId);
                    throw new NotFoundException($"Player with ID {playerId} not found");
                }

                var enemy = await _context.Enemies
                    .FirstOrDefaultAsync(e => e.Id == enemyId, cancellationToken);
                    
                if (enemy == null)
                {
                    _logger.LogWarning("Enemy not found with ID: {EnemyId}", enemyId);
                    throw new NotFoundException($"Enemy with ID {enemyId} not found");
                }

                // Добавляем врага в список побежденных, если его там еще нет
                if (!player.DefeatedEnemies.Any(e => e.Id == enemyId))
                {
                    player.DefeatedEnemies.Add(enemy);
                    player.Experience += enemy.ExperienceReward;
                    player.Gold += enemy.GoldReward;
                    player.UpdateTimestamps();
                    
                    await _context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Player {PlayerName} defeated enemy {EnemyName} and gained {ExperienceReward} experience and {GoldReward} gold",
                    player.Name, enemy.Name, enemy.ExperienceReward, enemy.GoldReward);
                    
                return _mapper.Map<PlayerDto>(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error defeating enemy for player with ID: {PlayerId}", playerId);
                throw;
            }
        }

        // Агрегация: количество игроков по уровням
        public async Task<Dictionary<int, int>> GetPlayersCountByLevelAsync(CancellationToken cancellationToken = default)
        {
            var counts = await _context.Players
                .AsNoTracking()
                .GroupBy(p => p.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count, cancellationToken);
            
            return counts;
        }

        // Агрегация: общее золото у всех игроков
        public async Task<int> GetTotalPlayerGoldAsync(CancellationToken cancellationToken = default)
        {
            var totalGold = await _context.Players
                .AsNoTracking()
                .SumAsync(p => p.Gold, cancellationToken);
            
            return totalGold;
        }

        // Агрегация: статистика по игрокам
        public async Task<PlayerStatsDto> GetPlayerStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = await _context.Players
                .AsNoTracking()
                .Select(p => new { p.Level, p.Gold, p.Health })
                .GroupBy(_ => 1) // Группируем все записи
                .Select(g => new PlayerStatsDto
                {
                    TotalPlayers = g.Count(),
                    AverageLevel = g.Average(p => p.Level),
                    TotalGold = g.Sum(p => p.Gold),
                    MaxLevel = g.Max(p => p.Level),
                    MinLevel = g.Min(p => p.Level)
                })
                .FirstOrDefaultAsync(cancellationToken);
            
            return stats ?? new PlayerStatsDto();
        }

public async Task<PlayerDto> CompleteQuestWithTransactionAsync(Guid playerId, Guid questId, CancellationToken cancellationToken = default)
{
    
    try
    {
        _logger.LogInformation("Starting operation for player {PlayerId} completing quest {QuestId}", playerId, questId);
        
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
            
        var quest = await _context.Quests
            .FirstOrDefaultAsync(q => q.Id == questId, cancellationToken);
        
        if (player == null || quest == null)
            throw new NotFoundException("Player or quest not found");
        
        // Обновляем прогресс квеста
        quest.UpdateProgress(1);
        _context.Quests.Update(quest);
        
        // Если квест завершен - награждаем игрока
        if (quest.IsCompleted)
        {
            player.CompleteQuest(quest);
            _context.Players.Update(player);
        }
        
        
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Operation completed successfully for player {PlayerId}", playerId);
        return _mapper.Map<PlayerDto>(player);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Operation failed for player {PlayerId} completing quest {QuestId}", playerId, questId);
        throw;
    }
}

    }
}