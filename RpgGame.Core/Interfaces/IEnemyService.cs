using RpgGame.Core.DTOs;
using RpgGame.Core.Models;

namespace RpgGame.Core.Interfaces
{
    public interface IEnemyService
    {
        Task<EnemyDto?> GetEnemyByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<EnemyDto>> GetEnemiesPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> SearchEnemiesAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> GetEnemiesByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> GetEnemiesByRewardRangeAsync(int minExp, int maxExp, int minGold, int maxGold, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> GetEnemiesByLocationAsync(string location, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> GetEnemiesByTypeAsync(EnemyType type, CancellationToken cancellationToken = default);
        Task<IEnumerable<EnemyDto>> GetAliveEnemiesAsync(CancellationToken cancellationToken = default);
        Task<EnemyDto> CreateEnemyAsync(CreateEnemyDto createDto, CancellationToken cancellationToken = default);
        Task<EnemyDto?> UpdateEnemyAsync(Guid id, UpdateEnemyDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteEnemyAsync(Guid id, CancellationToken cancellationToken = default);
        Task<EnemyDto> DamageEnemyAsync(Guid enemyId, int damage, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetEnemiesCountByTypeAsync(CancellationToken cancellationToken = default);
        Task<double> GetAverageEnemyLevelAsync(CancellationToken cancellationToken = default);
        Task<int> GetTotalGoldRewardAsync(CancellationToken cancellationToken = default);
    }
}