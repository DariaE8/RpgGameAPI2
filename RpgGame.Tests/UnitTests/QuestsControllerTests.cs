using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RpgGame.API.Controllers;
using RpgGame.Core.Interfaces;
using RpgGame.Core.DTOs;
using RpgGame.Core.Models;
using RpgGame.Core.Exceptions;

namespace RpgGame.Tests.UnitTests
{
    public class QuestsControllerTests
    {
        private readonly Mock<IQuestService> _mockQuestService;
        private readonly Mock<ILogger<QuestsController>> _mockLogger;
        private readonly QuestsController _controller;

        public QuestsControllerTests()
        {
            _mockQuestService = new Mock<IQuestService>();
            _mockLogger = new Mock<ILogger<QuestsController>>();
            _controller = new QuestsController(_mockQuestService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetQuest_WithValidId_ShouldReturnQuest()
        {
            // Arrange
            var questId = Guid.NewGuid();
            var questDto = new QuestDto { Id = questId, Title = "Test Quest" };
            _mockQuestService.Setup(s => s.GetQuestByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(questDto);

            // Act
            var result = await _controller.GetQuest(questId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedQuest = Assert.IsType<QuestDto>(okResult.Value);
            Assert.Equal(questId, returnedQuest.Id);
            Assert.Equal("Test Quest", returnedQuest.Title);
        }

        [Fact]
        public async Task GetQuest_WithInvalidId_ShouldThrowNotFoundException()
        {
            // Arrange
            var questId = Guid.NewGuid();
            _mockQuestService.Setup(s => s.GetQuestByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Quest not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetQuest(questId));
        }

        [Fact]
        public async Task CreateQuest_WithValidData_ShouldReturnCreatedQuest()
        {
            // Arrange
            var createDto = new CreateQuestDto { Title = "New Quest" };
            var questDto = new QuestDto { Id = Guid.NewGuid(), Title = "New Quest" };

            _mockQuestService.Setup(s => s.CreateQuestAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(questDto);

            // Act
            var result = await _controller.CreateQuest(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedQuest = Assert.IsType<QuestDto>(createdResult.Value);
            Assert.Equal("New Quest", returnedQuest.Title);
            Assert.Equal("GetQuest", createdResult.ActionName);
        }

        [Fact]
        public async Task UpdateQuest_WithValidData_ShouldReturnUpdatedQuest()
        {
            // Arrange
            var questId = Guid.NewGuid();
            var updateDto = new UpdateQuestDto { Title = "Updated Quest" };
            var questDto = new QuestDto { Id = questId, Title = "Updated Quest" };

            _mockQuestService.Setup(s => s.UpdateQuestAsync(questId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(questDto);

            // Act
            var result = await _controller.UpdateQuest(questId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedQuest = Assert.IsType<QuestDto>(okResult.Value);
            Assert.Equal("Updated Quest", returnedQuest.Title);
        }

        [Fact]
        public async Task GetQuestsByStatus_ShouldReturnQuestsByStatus()
        {
            // Arrange
            var quests = new List<QuestDto> 
            { 
                new QuestDto { Id = Guid.NewGuid(), Title = "In Progress", Status = "InProgress" }
            };
            _mockQuestService.Setup(s => s.GetQuestsByStatusAsync(QuestStatus.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(quests);

            // Act
            var result = await _controller.GetQuestsByStatus(QuestStatus.InProgress);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedQuests = Assert.IsType<List<QuestDto>>(okResult.Value);
            Assert.Single(returnedQuests);
            Assert.Equal("In Progress", returnedQuests.First().Title);
        }

        [Fact]
        public async Task DeleteQuest_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var questId = Guid.NewGuid();
            _mockQuestService.Setup(s => s.DeleteQuestAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteQuest(questId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteQuest_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var questId = Guid.NewGuid();
            _mockQuestService.Setup(s => s.DeleteQuestAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteQuest(questId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SearchQuests_WithEmptyTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchQuests("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetQuestsPaged_ShouldReturnPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<QuestDto>
            {
                Items = new List<QuestDto> 
                { 
                    new QuestDto { Id = Guid.NewGuid(), Title = "Quest 1" },
                    new QuestDto { Id = Guid.NewGuid(), Title = "Quest 2" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 20
            };

            _mockQuestService.Setup(s => s.GetQuestsPagedAsync(
                It.IsAny<PaginationDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetQuestsPaged();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<QuestDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.Items.Count());
            Assert.Equal(1, returnedResult.PageNumber);
            Assert.Equal(20, returnedResult.TotalCount);
        }

        [Fact]
        public async Task GetQuestsByExperienceRange_ShouldReturnQuestsInRange()
        {
            // Arrange
            var quests = new List<QuestDto>
            {
                new QuestDto { Id = Guid.NewGuid(), Title = "Easy Quest", ExperienceReward = 50 },
                new QuestDto { Id = Guid.NewGuid(), Title = "Medium Quest", ExperienceReward = 150 },
                new QuestDto { Id = Guid.NewGuid(), Title = "Hard Quest", ExperienceReward = 500 }
            };
            
            _mockQuestService.Setup(s => s.GetQuestsByExperienceRangeAsync(0, 200, It.IsAny<CancellationToken>()))
                .ReturnsAsync(quests.Where(q => q.ExperienceReward <= 200).ToList());

            // Act
            var result = await _controller.GetQuestsByExperienceRange(minExp: 0, maxExp: 200);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedQuests = Assert.IsType<List<QuestDto>>(okResult.Value);
            Assert.Equal(2, returnedQuests.Count);
            Assert.Contains(returnedQuests, q => q.Title == "Easy Quest");
            Assert.Contains(returnedQuests, q => q.Title == "Medium Quest");
            Assert.DoesNotContain(returnedQuests, q => q.Title == "Hard Quest");
        }

        [Fact]
        public async Task GetQuestsCountByStatus_ShouldReturnCounts()
        {
            // Arrange
            var counts = new Dictionary<string, int>
            {
                { "Available", 5 },
                { "InProgress", 3 },
                { "Completed", 10 }
            };
            
            _mockQuestService.Setup(s => s.GetQuestsCountByStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(counts);

            // Act
            var result = await _controller.GetQuestsCountByStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCounts = Assert.IsType<Dictionary<string, int>>(okResult.Value);
            Assert.Equal(3, returnedCounts.Count);
            Assert.Equal(5, returnedCounts["Available"]);
            Assert.Equal(3, returnedCounts["InProgress"]);
            Assert.Equal(10, returnedCounts["Completed"]);
        }

        [Fact]
        public async Task GetTotalQuestRewards_ShouldReturnTotalRewards()
        {
            // Arrange
            var rewards = new QuestRewardsDto
            {
                TotalExperience = 5000,
                TotalGold = 2500
            };
            
            _mockQuestService.Setup(s => s.GetTotalQuestRewardsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(rewards);

            // Act
            var result = await _controller.GetTotalQuestRewards();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRewards = Assert.IsType<QuestRewardsDto>(okResult.Value);
            Assert.Equal(5000, returnedRewards.TotalExperience);
            Assert.Equal(2500, returnedRewards.TotalGold);
        }

        [Fact]
public async Task SearchQuests_WithValidTerm_ShouldReturnMatchingQuests()
{
    // Arrange
    var quests = new List<QuestDto> 
    { 
        new QuestDto { Id = Guid.NewGuid(), Title = "Find the Lost Sword", Description = "Search for ancient sword" },
        new QuestDto { Id = Guid.NewGuid(), Title = "Defeat Dragon", Description = "Slay the dragon" }
    };
    
    _mockQuestService.Setup(s => s.SearchQuestsAsync("sword", It.IsAny<CancellationToken>()))
        .ReturnsAsync(quests);

    // Act
    var result = await _controller.SearchQuests("sword");

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedQuests = Assert.IsType<List<QuestDto>>(okResult.Value);
    Assert.Equal(2, returnedQuests.Count);
}

[Fact]
public async Task SearchQuests_WithNoResults_ShouldReturnEmptyList()
{
    // Arrange
    var emptyList = new List<QuestDto>();
    _mockQuestService.Setup(s => s.SearchQuestsAsync("nonexistent", It.IsAny<CancellationToken>()))
        .ReturnsAsync(emptyList);

    // Act
    var result = await _controller.SearchQuests("nonexistent");

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedQuests = Assert.IsType<List<QuestDto>>(okResult.Value);
    Assert.Empty(returnedQuests);
}

[Fact]
public async Task SearchQuests_WithWhitespaceTerm_ShouldReturnBadRequest()
{
    // Act
    var result = await _controller.SearchQuests("   ");

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    Assert.Equal("Search term is required", badRequestResult.Value);
}
    }
}