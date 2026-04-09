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
    public class EnemyService : IEnemyService
    {
        private readonly GameDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EnemyService> _logger;

        public EnemyService(
            GameDbContext context,
            IMapper mapper,
            ILogger<EnemyService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        
        public async Task<EnemyDto?> GetEnemyByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemy by ID: {EnemyId}", id);

            var enemy = await _context.Enemies
                .AsSplitQuery()
                .Include(e => e.GameLocation)
                .Include(e => e.DefeatedByPlayers)
                .Include(e => e.RequiredForQuests)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (enemy == null)
            {
                _logger.LogWarning("Enemy not found with ID: {EnemyId}", id);
                throw new KeyNotFoundException($"Enemy with id {id} not found");
            }

            _logger.LogInformation("Successfully retrieved enemy: {EnemyName}", enemy.Name);
            return _mapper.Map<EnemyDto>(enemy);
        }

        
        public async Task<PagedResult<EnemyDto>> GetEnemiesPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies paged - Page: {Page}, PageSize: {PageSize}",
                pagination.Page, pagination.PageSize);

            var query = _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                var lower = pagination.Search.ToLower();
                if (Enum.TryParse<EnemyType>(pagination.Search, true, out var parsedType))
                {
                    query = query.Where(e =>
                        e.Name.ToLower().Contains(lower) ||
                        (e.GameLocation != null && e.GameLocation.Name.ToLower().Contains(lower)) ||
                        e.Type == parsedType);
                }
                else
                {
                    query = query.Where(e =>
                        e.Name.ToLower().Contains(lower) ||
                        (e.GameLocation != null && e.GameLocation.Name.ToLower().Contains(lower)));
                }
            }

            query = pagination.SortBy?.ToLower() switch
            {
                "name" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
                "level" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.Level) : query.OrderBy(e => e.Level),
                "health" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.Health) : query.OrderBy(e => e.Health),
                "attack" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.Attack) : query.OrderBy(e => e.Attack),
                "experiencereward" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.ExperienceReward) : query.OrderBy(e => e.ExperienceReward),
                "createdat" => pagination.SortOrder == "desc" ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                _ => query.OrderBy(e => e.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<EnemyDto>
            {
                Items = _mapper.Map<List<EnemyDto>>(items),
                PageNumber = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

        
        public async Task<IEnumerable<EnemyDto>> SearchEnemiesAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching enemies with term: {SearchTerm}", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
                return Array.Empty<EnemyDto>();

            var lower = searchTerm.ToLower();

            var query = _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .AsQueryable();

            
            if (Enum.TryParse<EnemyType>(searchTerm, true, out var parsedType))
            {
                query = query.Where(e =>
                    e.Name.ToLower().Contains(lower) ||
                    (e.GameLocation != null && e.GameLocation.Name.ToLower().Contains(lower)) ||
                    e.Type == parsedType);
            }
            else
            {
                query = query.Where(e =>
                    e.Name.ToLower().Contains(lower) ||
                    (e.GameLocation != null && e.GameLocation.Name.ToLower().Contains(lower)));
            }

            
            var enemies = await query
                .Select(e => new EnemyDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Type = e.Type.ToString(),
                    Level = e.Level,
                    Health = e.Health,
                    MaxHealth = e.MaxHealth,
                    Attack = e.Attack,
                    ExperienceReward = e.ExperienceReward,
                    GoldReward = e.GoldReward,
                    Location = e.GameLocation != null ? e.GameLocation.Name : "Unknown Location",
                    IsAlive = e.Health > 0, 
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} enemies for search term: {SearchTerm}", enemies.Count, searchTerm);
            return enemies;
        }

        
        public async Task<IEnumerable<EnemyDto>> GetEnemiesByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);

            var lowerBound = Math.Min(minLevel, maxLevel);
            var upperBound = Math.Max(minLevel, maxLevel);

            var enemies = await _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .Where(e => e.Level >= lowerBound && e.Level <= upperBound)
                .Select(e => new EnemyDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Type = e.Type.ToString(),
                    Level = e.Level,
                    Health = e.Health,
                    MaxHealth = e.MaxHealth,
                    Attack = e.Attack,
                    ExperienceReward = e.ExperienceReward,
                    GoldReward = e.GoldReward,
                    Location = e.GameLocation != null ? e.GameLocation.Name : "Unknown Location",
                    IsAlive = e.Health > 0,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} enemies in level range {MinLevel}-{MaxLevel}",
                enemies.Count, minLevel, maxLevel);
            return enemies;
        }

        
        public async Task<IEnumerable<EnemyDto>> GetEnemiesByRewardRangeAsync(int minExp, int maxExp, int minGold, int maxGold, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by reward range - Exp: {MinExp}-{MaxExp}, Gold: {MinGold}-{MaxGold}",
                minExp, maxExp, minGold, maxGold);

            var enemies = await _context.Enemies
                .Include(e => e.GameLocation)
                .Where(e =>
                    e.ExperienceReward >= minExp && e.ExperienceReward <= maxExp &&
                    e.GoldReward >= minGold && e.GoldReward <= maxGold)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} enemies in specified reward range", enemies.Count());
            return _mapper.Map<IEnumerable<EnemyDto>>(enemies);
        }

        
        public async Task<IEnumerable<EnemyDto>> GetEnemiesByLocationAsync(string location, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by location: {Location}", location);

            if (string.IsNullOrWhiteSpace(location))
                return Array.Empty<EnemyDto>();

            var lower = location.ToLower();

            
            var enemies = await _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .Where(e => e.GameLocation != null && e.GameLocation.Name.ToLower() == lower)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} enemies in location: {Location}", enemies.Count(), location);
            return _mapper.Map<IEnumerable<EnemyDto>>(enemies);
        }

        
        public async Task<IEnumerable<EnemyDto>> GetEnemiesByTypeAsync(EnemyType type, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting enemies by type: {EnemyType}", type);

            var enemies = await _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .Where(e => e.Type == type)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} enemies of type: {EnemyType}", enemies.Count(), type);
            return _mapper.Map<IEnumerable<EnemyDto>>(enemies);
        }

        // ---------- Alive ----------
        public async Task<IEnumerable<EnemyDto>> GetAliveEnemiesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all alive enemies");

            // <<-- заменено e.IsAlive на e.Health > 0
            var enemies = await _context.Enemies
                .Include(e => e.GameLocation)
                .AsNoTracking()
                .Where(e => e.Health > 0)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} alive enemies", enemies.Count());
            return _mapper.Map<IEnumerable<EnemyDto>>(enemies);
        }

        // ---------- Create ----------
        public async Task<EnemyDto> CreateEnemyAsync(CreateEnemyDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new enemy with name: {EnemyName}", createDto.Name);

            try
            {
                // <<-- Use case-insensitive check using ToLower (works with InMemory and usual providers)
                var nameLower = createDto.Name.Trim().ToLower();
                var exists = await _context.Enemies.AnyAsync(e => e.Name.ToLower() == nameLower, cancellationToken);

                if (exists)
                {
                    _logger.LogWarning("Enemy creation failed - name already exists: {EnemyName}", createDto.Name);
                    throw new ConflictException($"Enemy with name '{createDto.Name}' already exists");
                }

                var enemy = _mapper.Map<Enemy>(createDto);

                _context.Enemies.Add(enemy);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully created enemy {EnemyName} with ID: {EnemyId}",
                    enemy.Name, enemy.Id);
                return _mapper.Map<EnemyDto>(enemy);
            }
            catch (Exception ex) when (!(ex is ConflictException))
            {
                _logger.LogError(ex, "Error creating enemy with name: {EnemyName}", createDto.Name);
                throw;
            }
        }

        // ---------- Update ----------
        public async Task<EnemyDto?> UpdateEnemyAsync(Guid id, UpdateEnemyDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating enemy with ID: {EnemyId}", id);

            try
            {
                var enemy = await _context.Enemies
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

                if (enemy == null)
                {
                    _logger.LogWarning("Enemy not found for update with ID: {EnemyId}", id);
                    // <<-- бросаем KeyNotFoundException чтобы контроллер мог поймать и вернуть NotFound()
                    throw new KeyNotFoundException("Enemy not found");
                }

                if (updateDto.Name != null && updateDto.Name != enemy.Name)
                {
                    var nameLower = updateDto.Name.Trim().ToLower();
                    var existingEnemy = await _context.Enemies
                        .FirstOrDefaultAsync(e => e.Name.ToLower() == nameLower && e.Id != id, cancellationToken);

                    if (existingEnemy != null)
                    {
                        _logger.LogWarning("Enemy update failed - name already exists: {EnemyName}", updateDto.Name);
                        throw new ConflictException($"Enemy with name '{updateDto.Name}' already exists");
                    }

                    enemy.Name = updateDto.Name;
                }

                if (updateDto.Type.HasValue) enemy.Type = updateDto.Type.Value;
                if (updateDto.Level.HasValue) enemy.Level = updateDto.Level.Value;
                if (updateDto.Health.HasValue) enemy.Health = updateDto.Health.Value;
                if (updateDto.MaxHealth.HasValue) enemy.MaxHealth = updateDto.MaxHealth.Value;
                if (updateDto.Attack.HasValue) enemy.Attack = updateDto.Attack.Value;
                if (updateDto.ExperienceReward.HasValue) enemy.ExperienceReward = updateDto.ExperienceReward.Value;
                if (updateDto.GoldReward.HasValue) enemy.GoldReward = updateDto.GoldReward.Value;
                if (updateDto.Location != null)
                {
                    var location = await _context.GameLocations
                        .FirstOrDefaultAsync(l => l.Name.ToLower() == updateDto.Location.ToLower(), cancellationToken);

                    if (location != null)
                    {
                        enemy.GameLocation = location; // assign entity
                    }
                }

                enemy.UpdateTimestamps();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully updated enemy {EnemyName} (ID: {EnemyId})",
                    enemy.Name, id);
                return _mapper.Map<EnemyDto>(enemy);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException) && !(ex is ConflictException))
            {
                _logger.LogError(ex, "Error updating enemy with ID: {EnemyId}", id);
                throw;
            }
        }

        // ---------- Delete ----------
        public async Task<bool> DeleteEnemyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting enemy with ID: {EnemyId}", id);

            var enemy = await _context.Enemies
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (enemy == null)
            {
                _logger.LogWarning("Enemy not found for deletion with ID: {EnemyId}", id);
                return false;
            }

            _context.Enemies.Remove(enemy);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted enemy with ID: {EnemyId}", id);
            return true;
        }

        // ---------- Damage ----------
        public async Task<EnemyDto> DamageEnemyAsync(Guid enemyId, int damage, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Applying {Damage} damage to enemy with ID: {EnemyId}", damage, enemyId);

            try
            {
                var enemy = await _context.Enemies
                    .FirstOrDefaultAsync(e => e.Id == enemyId, cancellationToken);

                if (enemy == null)
                {
                    _logger.LogWarning("Enemy not found for damage with ID: {EnemyId}", enemyId);
                    throw new KeyNotFoundException("Enemy not found");
                }

                var originalHealth = enemy.Health;
                enemy.TakeDamage(damage);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Applied {Damage} damage to enemy {EnemyName}. Health: {OriginalHealth} -> {CurrentHealth}",
                    damage, enemy.Name, originalHealth, enemy.Health);

                if (enemy.Health <= 0)
                {
                    _logger.LogInformation("Enemy {EnemyName} has been defeated", enemy.Name);
                }

                return _mapper.Map<EnemyDto>(enemy);
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error applying damage to enemy with ID: {EnemyId}", enemyId);
                throw;
            }
        }

        // ---------- Aggregations ----------
        public async Task<Dictionary<string, int>> GetEnemiesCountByTypeAsync(CancellationToken cancellationToken = default)
        {
            var counts = await _context.Enemies
                .AsNoTracking()
                .GroupBy(e => e.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

            return counts;
        }

        public async Task<double> GetAverageEnemyLevelAsync(CancellationToken cancellationToken = default)
        {
            var averageLevel = await _context.Enemies
                .AsNoTracking()
                .AverageAsync(e => (double)e.Level, cancellationToken);

            return Math.Round(averageLevel, 2);
        }

        public async Task<int> GetTotalGoldRewardAsync(CancellationToken cancellationToken = default)
        {
            var totalGold = await _context.Enemies
                .AsNoTracking()
                .SumAsync(e => e.GoldReward, cancellationToken);

            return totalGold;
        }
    }
}
