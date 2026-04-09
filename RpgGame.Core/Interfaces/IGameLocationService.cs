using RpgGame.Core.DTOs;
using RpgGame.Core.Models;

namespace RpgGame.Core.Interfaces
{
    public interface IGameLocationService
    {
        Task<GameLocationDto?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<GameLocationDto>> GetLocationsPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> SearchLocationsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetLocationsByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetAccessibleLocationsAsync(int playerLevel, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetSafeZonesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetLocationsByTypeAsync(LocationType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetLocationsWithEnemiesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<GameLocationDto>> GetLocationsWithQuestsAsync(CancellationToken cancellationToken = default);
        Task<GameLocationDto> CreateLocationAsync(CreateGameLocationDto createDto, CancellationToken cancellationToken = default);
        Task<GameLocationDto?> UpdateLocationAsync(Guid id, UpdateGameLocationDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetPlayerDistributionAsync(CancellationToken cancellationToken = default);
    }
}