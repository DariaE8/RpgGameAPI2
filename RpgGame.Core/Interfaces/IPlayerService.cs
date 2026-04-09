using RpgGame.Core.DTOs;
using RpgGame.Core.Models;

namespace RpgGame.Core.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerDto> GetPlayerByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<PlayerDto>> GetPlayersPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlayerDto>> SearchPlayersAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlayerDto>> GetPlayersByLevelRangeAsync(int minLevel, int maxLevel, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlayerDto>> GetAlivePlayersAsync(CancellationToken cancellationToken = default);
        Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createDto, CancellationToken cancellationToken = default);
        Task<PlayerDto> UpdatePlayerAsync(Guid id, UpdatePlayerDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PlayerDto> AddExperienceAsync(Guid playerId, int experience, CancellationToken cancellationToken = default);
        Task<PlayerDto> HealPlayerAsync(Guid playerId, int amount, CancellationToken cancellationToken = default);
        Task<PlayerDto> DamagePlayerAsync(Guid playerId, int damage, CancellationToken cancellationToken = default);
        Task<PlayerDto> CompleteQuestAsync(Guid playerId, Guid questId, CancellationToken cancellationToken = default);
        Task<PlayerDto> DefeatEnemyAsync(Guid playerId, Guid enemyId, CancellationToken cancellationToken = default);
        Task<Dictionary<int, int>> GetPlayersCountByLevelAsync(CancellationToken cancellationToken = default);
        Task<int> GetTotalPlayerGoldAsync(CancellationToken cancellationToken = default);
        Task<PlayerStatsDto> GetPlayerStatsAsync(CancellationToken cancellationToken = default);
        Task<PlayerDto> CompleteQuestWithTransactionAsync(Guid playerId, Guid questId, CancellationToken cancellationToken = default);
    }
}