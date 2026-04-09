using RpgGame.Core.DTOs;
using RpgGame.Core.Models;

namespace RpgGame.Core.Interfaces
{
    public interface IQuestService
    {
        Task<QuestDto?> GetQuestByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<QuestDto>> GetQuestsPagedAsync(PaginationDto pagination, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestDto>> SearchQuestsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestDto>> GetQuestsByExperienceRangeAsync(int minExp, int maxExp, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestDto>> GetAvailableQuestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestDto>> GetQuestsByStatusAsync(QuestStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestDto>> GetCompletedQuestsAsync(CancellationToken cancellationToken = default);
        Task<QuestDto> CreateQuestAsync(CreateQuestDto createDto, CancellationToken cancellationToken = default);
        Task<QuestDto?> UpdateQuestAsync(Guid id, UpdateQuestDto updateDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteQuestAsync(Guid id, CancellationToken cancellationToken = default);
        Task<QuestDto?> UpdateQuestProgressAsync(Guid questId, int progress = 1, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetQuestsCountByStatusAsync(CancellationToken cancellationToken = default);
        Task<QuestRewardsDto> GetTotalQuestRewardsAsync(CancellationToken cancellationToken = default);
    }
}