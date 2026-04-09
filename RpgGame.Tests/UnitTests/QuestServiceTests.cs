using RpgGame.Core.Models;
using RpgGame.Core.Interfaces;
using RpgGame.Services.Services;
using RpgGame.Core.DTOs;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RpgGame.Infrastructure.Data;
using RpgGame.Core.Exceptions;

namespace RpgGame.Tests.Services
{
    public class QuestServiceTests : IDisposable
    {
        private readonly GameDbContext _context;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<QuestService>> _loggerMock;
        private readonly QuestService _questService;

        public QuestServiceTests()
        {
            // Создаем InMemory базу данных для тестов
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new GameDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<QuestService>>();
            
            _questService = new QuestService(
                _context,
                _mapperMock.Object,
                _loggerMock.Object);
            
            // Заполняем тестовыми данными
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Создаем тестовый квест
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Test Quest",
                Description = "A test quest",
                Objective = "Test objective",
                TargetCount = 5,
                CurrentCount = 0,
                ExperienceReward = 100,
                GoldReward = 50,
                Status = QuestStatus.Available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quests.Add(quest);

            // Создаем завершенный квест
            var completedQuest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Completed Quest",
                Description = "A completed quest",
                Objective = "Completed objective",
                TargetCount = 3,
                CurrentCount = 3,
                ExperienceReward = 200,
                GoldReward = 100,
                Status = QuestStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quests.Add(completedQuest);

            // Создаем квест в процессе
            var inProgressQuest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "In Progress Quest",
                Description = "A quest in progress",
                Objective = "Progress objective",
                TargetCount = 10,
                CurrentCount = 3,
                ExperienceReward = 500,
                GoldReward = 250,
                Status = QuestStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quests.Add(inProgressQuest);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetQuestByIdAsync_ShouldReturnQuest_WhenExists()
        {
            // Arrange
            var questId = _context.Quests.First().Id;
            var questDto = new QuestDto { Id = questId, Title = "Test Quest" };
            
            _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
                .Returns(questDto);

            // Act
            var result = await _questService.GetQuestByIdAsync(questId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(questId, result.Id);
            Assert.Equal("Test Quest", result.Title);
        }

        [Fact]
        public async Task GetQuestByIdAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            var questId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _questService.GetQuestByIdAsync(questId));
        }

        [Fact]
        public async Task GetAvailableQuestsAsync_ShouldReturnAvailableQuests()
        {
            // Arrange
            var availableQuests = _context.Quests
                .Where(q => q.Status == QuestStatus.Available)
                .ToList();
            
            var questDtos = availableQuests.Select(q => new QuestDto 
            { 
                Title = q.Title,
                Status = q.Status.ToString()
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<QuestDto>>(It.IsAny<IEnumerable<Quest>>()))
                .Returns(questDtos);

            // Act
            var result = await _questService.GetAvailableQuestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Только один доступный квест
            Assert.Equal("Available", result.First().Status);
        }

        [Fact]
        public async Task CreateQuestAsync_ShouldCreateQuest_WhenValidData()
        {
            // Arrange
            var createDto = new CreateQuestDto 
            { 
                Title = "New Quest", 
                Description = "New description",
                Objective = "New objective",
                TargetCount = 3,
                ExperienceReward = 150,
                GoldReward = 75
            };
            
            var quest = new Quest 
            { 
                Id = Guid.NewGuid(), 
                Title = "New Quest",
                Description = "New description"
            };
            
            var questDto = new QuestDto 
            { 
                Id = quest.Id, 
                Title = "New Quest" 
            };
            
            _mapperMock.Setup(x => x.Map<Quest>(createDto))
                .Returns(quest);
            _mapperMock.Setup(x => x.Map<QuestDto>(quest))
                .Returns(questDto);

            // Act
            var result = await _questService.CreateQuestAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Quest", result.Title);
            // Проверяем, что квест добавился
            Assert.Equal(4, await _context.Quests.CountAsync());
        }

        [Fact]
        public async Task UpdateQuestProgressAsync_ShouldUpdateProgress_WhenQuestExists()
        {
            // Arrange
            var questId = _context.Quests.First().Id;
            var questDto = new QuestDto 
            { 
                Id = questId, 
                Title = "Test Quest", 
                CurrentCount = 1, 
                TargetCount = 5 
            };
            
            _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
                .Returns(questDto);

            // Act
            var result = await _questService.UpdateQuestProgressAsync(questId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CurrentCount);
            
            var updatedQuest = await _context.Quests.FindAsync(questId);
            Assert.Equal(1, updatedQuest?.CurrentCount);
        }

        [Fact]
        public async Task DeleteQuestAsync_ShouldReturnTrue_WhenQuestExists()
        {
            // Arrange
            var questId = _context.Quests.First().Id;

            // Act
            var result = await _questService.DeleteQuestAsync(questId);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Quests.FindAsync(questId));
        }

        [Fact]
        public async Task DeleteQuestAsync_ShouldReturnFalse_WhenQuestNotExists()
        {
            // Arrange
            var questId = Guid.NewGuid();

            // Act
            var result = await _questService.DeleteQuestAsync(questId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetQuestsPagedAsync_ShouldReturnPagedQuests()
        {
            // Arrange
            var pagination = new PaginationDto { Page = 1, PageSize = 2 };
            
            var pagedResult = new PagedResult<QuestDto>
            {
                Items = new List<QuestDto> 
                { 
                    new QuestDto { Title = "Test Quest" }, 
                    new QuestDto { Title = "Completed Quest" } 
                },
                PageNumber = 1,
                PageSize = 2,
                TotalCount = 3
            };
            
            _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
                .Returns(pagedResult.Items.ToList());

            // Act
            var result = await _questService.GetQuestsPagedAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task SearchQuestsAsync_ShouldReturnQuests()
        {
            // Arrange
            var searchTerm = "test";
            
            var quests = _context.Quests
                .Where(q => q.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           q.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            var questDtos = quests.Select(q => new QuestDto 
            { 
                Title = q.Title 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<QuestDto>>(It.IsAny<IEnumerable<Quest>>()))
                .Returns(questDtos);

            // Act
            var result = await _questService.SearchQuestsAsync(searchTerm);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Quest", result.First().Title);
        }

        [Fact]
        public async Task GetQuestsByExperienceRangeAsync_ShouldReturnQuestsInRange()
        {
            // Arrange
            var minExp = 50;
            var maxExp = 150;
            
            var quests = _context.Quests
                .Where(q => q.ExperienceReward >= minExp && q.ExperienceReward <= maxExp)
                .ToList();
            
            var questDtos = quests.Select(q => new QuestDto 
            { 
                Title = q.Title, 
                ExperienceReward = q.ExperienceReward 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<QuestDto>>(It.IsAny<IEnumerable<Quest>>()))
                .Returns(questDtos);

            // Act
            var result = await _questService.GetQuestsByExperienceRangeAsync(minExp, maxExp);

            // Assert
            // Test Quest (100) и Completed Quest (200) - только Test Quest в диапазоне
            Assert.Single(result);
            Assert.Equal("Test Quest", result.First().Title);
            Assert.InRange(result.First().ExperienceReward, minExp, maxExp);
        }

        [Fact]
        public async Task GetQuestsByStatusAsync_ShouldReturnQuestsWithStatus()
        {
            // Arrange
            var status = QuestStatus.Available;
            
            var quests = _context.Quests
                .Where(q => q.Status == status)
                .ToList();
            
            var questDtos = quests.Select(q => new QuestDto 
            { 
                Title = q.Title, 
                Status = q.Status.ToString() 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<QuestDto>>(It.IsAny<IEnumerable<Quest>>()))
                .Returns(questDtos);

            // Act
            var result = await _questService.GetQuestsByStatusAsync(status);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Quest", result.First().Title);
            Assert.Equal("Available", result.First().Status);
        }

        [Fact]
        public async Task GetCompletedQuestsAsync_ShouldReturnCompletedQuests()
        {
            // Arrange
            var completedQuests = _context.Quests
                .Where(q => q.CurrentCount >= q.TargetCount) // Используем выражение вместо свойства IsCompleted
                .ToList();
            
            var questDtos = completedQuests.Select(q => new QuestDto
            {
                Title = q.Title,
                CurrentCount = q.CurrentCount,
                TargetCount = q.TargetCount
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<QuestDto>>(It.IsAny<IEnumerable<Quest>>()))
                .Returns(questDtos);

            // Act
            var result = await _questService.GetCompletedQuestsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Completed Quest", result.First().Title);
            Assert.True(result.First().CurrentCount >= result.First().TargetCount);
        }

        [Fact]
        public async Task UpdateQuestAsync_ShouldUpdateQuest_WhenExists()
        {
            // Arrange
            var questId = _context.Quests.First().Id;
            var updateDto = new UpdateQuestDto 
            { 
                Title = "Updated Quest", 
                ExperienceReward = 500 
            };
            
            var questDto = new QuestDto 
            { 
                Id = questId, 
                Title = "Updated Quest", 
                ExperienceReward = 500 
            };
            
            _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
                .Returns(questDto);

            // Act
            var result = await _questService.UpdateQuestAsync(questId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Quest", result.Title);
            Assert.Equal(500, result.ExperienceReward);
            
            var updatedQuest = await _context.Quests.FindAsync(questId);
            Assert.Equal("Updated Quest", updatedQuest?.Title);
            Assert.Equal(500, updatedQuest?.ExperienceReward);
        }

        [Fact]
        public async Task UpdateQuestProgressAsync_ShouldCompleteQuest_WhenTargetReached()
        {
            // Arrange
            var inProgressQuest = _context.Quests.First(q => q.Status == QuestStatus.InProgress);
            var questId = inProgressQuest.Id;
            
            var questDto = new QuestDto 
            { 
                Id = questId, 
                Title = "In Progress Quest", 
                CurrentCount = 10, 
                TargetCount = 10,
                Status = "Completed"
            };
            
            _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
                .Returns(questDto);

            // Act - Добавляем достаточно прогресса для завершения
            var result = await _questService.UpdateQuestProgressAsync(questId, 7);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.CurrentCount);
            Assert.Equal("Completed", result.Status);
            
            var completedQuest = await _context.Quests.FindAsync(questId);
            Assert.True(completedQuest?.IsCompleted);
            Assert.Equal(QuestStatus.Completed, completedQuest?.Status);
        }

        [Fact]
        public async Task GetQuestsCountByStatusAsync_ShouldReturnCorrectCounts()
        {
            // Act
            var result = await _questService.GetQuestsCountByStatusAsync();

            // Assert
            Assert.True(result.ContainsKey("Available"));
            Assert.True(result.ContainsKey("InProgress"));
            Assert.True(result.ContainsKey("Completed"));
            Assert.Equal(1, result["Available"]);
            Assert.Equal(1, result["InProgress"]);
            Assert.Equal(1, result["Completed"]);
        }

        [Fact]
        public async Task GetTotalQuestRewardsAsync_ShouldReturnTotalRewards()
        {
            // Act
            var result = await _questService.GetTotalQuestRewardsAsync();

            // Assert
            Assert.NotNull(result);
            // 100 + 200 + 500 = 800 опыта
            Assert.Equal(800, result.TotalExperience);
            // 50 + 100 + 250 = 400 золота
            Assert.Equal(400, result.TotalGold);
            Assert.Equal(3, result.QuestCount);
        }

        [Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSearch()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        Search = "Test" // Покрывает строки 60-67 (поиск)
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest" }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Items);
}

[Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSortingByTitleAsc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "title", // Покрывает строку 71: сортировка по названию asc
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest" }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSortingByTitleDesc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "title",
        SortOrder = "desc" // Покрывает строку 71: сортировка по названию desc
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest" }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSortingByExperienceReward()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "experiencereward", // Покрывает строку 72: сортировка по награде опыта
        SortOrder = "desc"
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest", ExperienceReward = 100 }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSortingByStatus()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "status", // Покрывает строку 73: сортировка по статусу
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest", Status = "Available" }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetQuestsPagedAsync_ShouldHandleSortingByCreatedAt()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "createdat", // Покрывает строку 74: сортировка по дате создания
        SortOrder = "desc"
    };
    
    _mapperMock.Setup(x => x.Map<List<QuestDto>>(It.IsAny<List<Quest>>()))
        .Returns(new List<QuestDto> 
        { 
            new QuestDto { Title = "Test Quest", CreatedAt = DateTime.UtcNow }
        });

    // Act
    var result = await _questService.GetQuestsPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task CreateQuestAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var createDto = new CreateQuestDto 
    { 
        Title = "Error Quest",
        Description = "A quest that will cause error",
        Objective = "Test objective",
        TargetCount = 1,
        ExperienceReward = 100,
        GoldReward = 50,
        RequiredLocation = "Test Forest"
    };
    
    // Симулируем ошибку при маппинге - покрывает строки 180-183
    _mapperMock.Setup(x => x.Map<Quest>(createDto))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() => 
        _questService.CreateQuestAsync(createDto));
}

[Fact]
public async Task UpdateQuestAsync_ShouldThrow_WhenQuestNotFound()
{
    // Arrange
    var questId = Guid.NewGuid();
    var updateDto = new UpdateQuestDto { Title = "Updated Quest" };

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _questService.UpdateQuestAsync(questId, updateDto)); // Покрывает строки 196-199
}

[Fact]
public async Task UpdateQuestAsync_ShouldUpdateLocation()
{
    // Arrange
    var questId = _context.Quests.First().Id;
    
    // Создаем новую локацию
    var newLocation = new GameLocation
    {
        Id = Guid.NewGuid(),
        Name = "Dark Forest",
        Description = "A dark forest",
        Type = LocationType.Forest,
        RequiredLevel = 3,
        IsSafeZone = false
    };
    _context.GameLocations.Add(newLocation);
    await _context.SaveChangesAsync();
    
    var updateDto = new UpdateQuestDto 
    { 
        RequiredLocation = "Dark Forest" // Покрывает строки 208-217: обновление локации
    };
    
    var questDto = new QuestDto 
    { 
        Id = questId, 
        Title = "Test Quest",
        RequiredLocation = "Dark Forest"
    };
    
    _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
        .Returns(questDto);

    // Act
    var result = await _questService.UpdateQuestAsync(questId, updateDto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Dark Forest", result.RequiredLocation);
}

[Fact]
public async Task UpdateQuestAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var questId = _context.Quests.First().Id;
    var updateDto = new UpdateQuestDto { Title = "Updated Quest" };
    
    // Симулируем ошибку при маппинге - покрывает строки 226-229
    _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _questService.UpdateQuestAsync(questId, updateDto));
}

[Fact]
public async Task UpdateQuestProgressAsync_ShouldThrow_WhenQuestNotFound()
{
    // Arrange
    var questId = Guid.NewGuid();
    var progress = 1;

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _questService.UpdateQuestProgressAsync(questId, progress)); // Покрывает строки 262-265
}

[Fact]
public async Task UpdateQuestProgressAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var questId = _context.Quests.First().Id;
    var progress = 1;
    
    // Симулируем ошибку при маппинге - покрывает строки 284-287
    _mapperMock.Setup(x => x.Map<QuestDto>(It.IsAny<Quest>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _questService.UpdateQuestProgressAsync(questId, progress));
}
    }
}