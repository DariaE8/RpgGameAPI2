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
    public class GameLocationService : IGameLocationService
    {
        private readonly GameDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GameLocationService> _logger;

        public GameLocationService(
            GameDbContext context,
            IMapper mapper,
            ILogger<GameLocationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Dictionary<string, int>> GetPlayerDistributionAsync(CancellationToken cancellationToken = default)
        {
            var distribution = await _context.Players
                .Include(p => p.CurrentGameLocation)
                .AsNoTracking()
                .Select(p => new 
                { 
                    LocationName = p.CurrentGameLocation != null ? p.CurrentGameLocation.Name : null 
                })
                .Where(x => x.LocationName != null) // 🔥 ФИЛЬТРУЕМ ПОСЛЕ ПРОЕКЦИИ
                .GroupBy(x => x.LocationName!)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Location, x => x.Count, cancellationToken);
            
            return distribution;
        }

            
        public async Task<GameLocationDto?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting location by ID: {LocationId}", id);
            
            var location = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Include(l => l.Players)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
                
            if (location == null)
            {
                _logger.LogWarning("Location not found with ID: {LocationId}", id);
                throw new NotFoundException($"Location with ID {id} not found");
            }
            
            _logger.LogInformation("Successfully retrieved location: {LocationName}", location.Name);
            return _mapper.Map<GameLocationDto>(location);
        }

        public async Task<PagedResult<GameLocationDto>> GetLocationsPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations paged - Page: {Page}, PageSize: {PageSize}", 
                pagination.Page, pagination.PageSize);
            
            var query = _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(l => 
                    l.Name.Contains(pagination.Search) ||
                    l.Description.Contains(pagination.Search) ||
                    l.Type.ToString().Contains(pagination.Search)
                );
            }

            query = pagination.SortBy?.ToLower() switch
            {
                "name" => pagination.SortOrder == "desc" ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
                "requiredlevel" => pagination.SortOrder == "desc" ? query.OrderByDescending(l => l.RequiredLevel) : query.OrderBy(l => l.RequiredLevel),
                "type" => pagination.SortOrder == "desc" ? query.OrderByDescending(l => l.Type) : query.OrderBy(l => l.Type),
                "createdat" => pagination.SortOrder == "desc" ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
                _ => query.OrderBy(l => l.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<GameLocationDto>
            {
                Items = _mapper.Map<List<GameLocationDto>>(items),
                PageNumber = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<GameLocationDto>> SearchLocationsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching locations with term: {SearchTerm}", searchTerm);
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .AsNoTracking()
                .Where(l => 
                    l.Name.Contains(searchTerm) ||
                    l.Description.Contains(searchTerm) ||
                    l.Type.ToString().Contains(searchTerm))
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} locations for search term: {SearchTerm}", 
                locations.Count(), searchTerm);
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetLocationsByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations by level range: {MinLevel}-{MaxLevel}", minLevel, maxLevel);
            
            var locations = await _context.GameLocations
                .AsSplitQuery() 
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.RequiredLevel >= minLevel && l.RequiredLevel <= maxLevel)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} locations in level range {MinLevel}-{MaxLevel}", 
                locations.Count(), minLevel, maxLevel);
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetAccessibleLocationsAsync(int playerLevel, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting accessible locations for player level: {PlayerLevel}", playerLevel);
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.RequiredLevel <= playerLevel) // Используем выражение вместо метода CanPlayerAccess
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} accessible locations for level {PlayerLevel}", 
                locations.Count(), playerLevel);
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetSafeZonesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all safe zones");
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.IsSafeZone)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} safe zones", locations.Count());
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetLocationsByTypeAsync(LocationType type, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations by type: {LocationType}", type);
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.Type == type)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} locations of type {LocationType}", locations.Count(), type);
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetLocationsWithEnemiesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations with enemies");
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.Enemies.Any()) // Используем выражение вместо метода HasEnemies()
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} locations with enemies", locations.Count());
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<IEnumerable<GameLocationDto>> GetLocationsWithQuestsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting locations with quests");
            
            var locations = await _context.GameLocations
                .AsSplitQuery()
                .Include(l => l.Enemies)
                .Include(l => l.Quests)
                .Where(l => l.Quests.Any())
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} locations with quests", locations.Count());
            return _mapper.Map<IEnumerable<GameLocationDto>>(locations);
        }

        public async Task<GameLocationDto> CreateLocationAsync(CreateGameLocationDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new location with name: {LocationName}", createDto.Name);
            
            try
            {
                var location = _mapper.Map<GameLocation>(createDto);
                
                _context.GameLocations.Add(location);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully created location {LocationName} with ID: {LocationId}", 
                    location.Name, location.Id);
                return _mapper.Map<GameLocationDto>(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location with name: {LocationName}", createDto.Name);
                throw;
            }
        }

        public async Task<GameLocationDto?> UpdateLocationAsync(Guid id, UpdateGameLocationDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating location with ID: {LocationId}", id);
            
            try
            {
                var location = await _context.GameLocations
                    .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
                    
                if (location == null)
                {
                    _logger.LogWarning("Location not found for update with ID: {LocationId}", id);
                    throw new NotFoundException($"Location with ID {id} not found");
                }

                if (updateDto.Name != null) location.Name = updateDto.Name;
                if (updateDto.Description != null) location.Description = updateDto.Description;
                if (updateDto.Type.HasValue) location.Type = updateDto.Type.Value;
                if (updateDto.RequiredLevel.HasValue) location.RequiredLevel = updateDto.RequiredLevel.Value;
                if (updateDto.IsSafeZone.HasValue) location.IsSafeZone = updateDto.IsSafeZone.Value;

                location.UpdateTimestamps();
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated location {LocationName} (ID: {LocationId})", 
                    location.Name, id);
                return _mapper.Map<GameLocationDto>(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location with ID: {LocationId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting location with ID: {LocationId}", id);
            
            var location = await _context.GameLocations
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
                
            if (location == null)
            {
                _logger.LogWarning("Location not found for deletion with ID: {LocationId}", id);
                return false;
            }

            _context.GameLocations.Remove(location);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully deleted location with ID: {LocationId}", id);
            return true;
        }

        
    }
}
