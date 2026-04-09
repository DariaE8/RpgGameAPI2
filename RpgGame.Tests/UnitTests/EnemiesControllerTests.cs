using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RpgGame.API.Controllers;
using RpgGame.Core.Interfaces;
using RpgGame.Core.DTOs;
using RpgGame.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace RpgGame.Tests.UnitTests
{
    public class EnemiesControllerTests
    {
        private readonly Mock<IEnemyService> _mockEnemyService;
        private readonly Mock<ILogger<EnemiesController>> _mockLogger;
        private readonly EnemiesController _controller;

        public EnemiesControllerTests()
        {
            _mockEnemyService = new Mock<IEnemyService>();
            _mockLogger = new Mock<ILogger<EnemiesController>>();
            _controller = new EnemiesController(_mockEnemyService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetEnemy_WithValidId_ShouldReturnEnemy()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            var enemyDto = new EnemyDto { Id = enemyId, Name = "TestEnemy", Type = "Goblin" };
            _mockEnemyService.Setup(s => s.GetEnemyByIdAsync(enemyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemyDto);

            // Act
            var result = await _controller.GetEnemy(enemyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemy = Assert.IsType<EnemyDto>(okResult.Value);
            Assert.Equal(enemyId, returnedEnemy.Id);
            Assert.Equal("TestEnemy", returnedEnemy.Name);
            Assert.Equal("Goblin", returnedEnemy.Type);
        }

        [Fact]
        public async Task GetEnemy_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            _mockEnemyService.Setup(s => s.GetEnemyByIdAsync(enemyId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetEnemy(enemyId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetEnemiesPaged_WithValidParameters_ShouldReturnPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<EnemyDto>
            {
                Items = new List<EnemyDto> 
                { 
                    new EnemyDto { Id = Guid.NewGuid(), Name = "Enemy1", Level = 5 }
                },
                PageNumber = 1, // 🔥 ИСПРАВЛЕНО: PageNumber вместо Page
                PageSize = 10,
                TotalCount = 15
                // TotalPages вычисляется автоматически
            };

            _mockEnemyService.Setup(s => s.GetEnemiesPagedAsync(
                It.Is<PaginationDto>(p => 
                    p.Page == 1 && 
                    p.PageSize == 10 && 
                    p.SortBy == "level" && 
                    p.SortOrder == "desc" &&
                    p.Search == "goblin"), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetEnemiesPaged(1, 10, "level", "desc", "goblin");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPagedResult = Assert.IsType<PagedResult<EnemyDto>>(okResult.Value);
            Assert.Single(returnedPagedResult.Items);
            Assert.Equal(1, returnedPagedResult.PageNumber); // 🔥 ИСПРАВЛЕНО
            Assert.Equal(10, returnedPagedResult.PageSize);
            Assert.Equal(15, returnedPagedResult.TotalCount);
            Assert.Equal(2, returnedPagedResult.TotalPages); // 15 / 10 = 1.5 → округляем до 2
            Assert.True(returnedPagedResult.HasNextPage);
            Assert.False(returnedPagedResult.HasPreviousPage);
        }

        [Fact]
        public async Task SearchEnemies_WithValidTerm_ShouldReturnMatchingEnemies()
        {
            // Arrange
            var enemies = new List<EnemyDto> 
            { 
                new EnemyDto { Id = Guid.NewGuid(), Name = "Forest Goblin", Location = "forest" }
            };
            _mockEnemyService.Setup(s => s.SearchEnemiesAsync("goblin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemies);

            // Act
            var result = await _controller.SearchEnemies("goblin");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemies = Assert.IsType<List<EnemyDto>>(okResult.Value);
            Assert.Single(returnedEnemies);
            Assert.Equal("Forest Goblin", returnedEnemies.First().Name);
        }

        [Fact]
        public async Task SearchEnemies_WithEmptyTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchEnemies("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchEnemies_WithWhitespaceTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchEnemies("   ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetEnemiesByLevelRange_ShouldReturnEnemiesInRange()
        {
            // Arrange
            var enemies = new List<EnemyDto>
            {
                new EnemyDto { Id = Guid.NewGuid(), Name = "Weak Enemy", Level = 3 },
                new EnemyDto { Id = Guid.NewGuid(), Name = "Medium Enemy", Level = 7 }
            };
            _mockEnemyService.Setup(s => s.GetEnemiesByLevelRangeAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemies);

            // Act
            var result = await _controller.GetEnemiesByLevelRange(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemies = Assert.IsType<List<EnemyDto>>(okResult.Value);
            Assert.Equal(2, returnedEnemies.Count);
        }

        [Fact]
        public async Task GetEnemiesByRewardRange_ShouldReturnEnemiesInRewardRange()
        {
            // Arrange
            var enemies = new List<EnemyDto>
            {
                new EnemyDto { Id = Guid.NewGuid(), Name = "Low Reward", ExperienceReward = 25, GoldReward = 10 }
            };
            _mockEnemyService.Setup(s => s.GetEnemiesByRewardRangeAsync(20, 30, 5, 15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemies);

            // Act
            var result = await _controller.GetEnemiesByRewardRange(20, 30, 5, 15);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemies = Assert.IsType<List<EnemyDto>>(okResult.Value);
            Assert.Single(returnedEnemies);
            Assert.Equal("Low Reward", returnedEnemies.First().Name);
        }

        [Fact]
        public async Task CreateEnemy_WithValidData_ShouldReturnCreatedEnemy()
        {
            // Arrange
            var createDto = new CreateEnemyDto 
            { 
                Name = "New Goblin", 
                Type = EnemyType.Goblin,
                Level = 3,
                Health = 40,
                MaxHealth = 40,
                Attack = 8
            };
            var enemyDto = new EnemyDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "New Goblin", 
                Type = "Goblin",
                Level = 3,
                Health = 40,
                MaxHealth = 40,
                Attack = 8
            };

            _mockEnemyService.Setup(s => s.CreateEnemyAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemyDto);

            // Act
            var result = await _controller.CreateEnemy(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(EnemiesController.GetEnemy), createdResult.ActionName);
            
            Assert.NotNull(createdResult.RouteValues);
            Assert.True(createdResult.RouteValues.ContainsKey("id"));
            Assert.Equal(enemyDto.Id, createdResult.RouteValues["id"]);
            
            var returnedEnemy = Assert.IsType<EnemyDto>(createdResult.Value);
            Assert.Equal("New Goblin", returnedEnemy.Name);
            Assert.Equal("Goblin", returnedEnemy.Type);
            Assert.Equal(3, returnedEnemy.Level);
        }

        [Fact]
        public async Task UpdateEnemy_WithValidData_ShouldReturnUpdatedEnemy()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            var updateDto = new UpdateEnemyDto 
            { 
                Name = "Updated Goblin",
                Level = 5,
                Health = 60
            };
            var enemyDto = new EnemyDto 
            { 
                Id = enemyId, 
                Name = "Updated Goblin", 
                Level = 5,
                Health = 60
            };

            _mockEnemyService.Setup(s => s.UpdateEnemyAsync(enemyId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemyDto);

            // Act
            var result = await _controller.UpdateEnemy(enemyId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemy = Assert.IsType<EnemyDto>(okResult.Value);
            Assert.Equal("Updated Goblin", returnedEnemy.Name);
            Assert.Equal(5, returnedEnemy.Level);
            Assert.Equal(60, returnedEnemy.Health);
        }

        [Fact]
        public async Task UpdateEnemy_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            var updateDto = new UpdateEnemyDto { Name = "Updated Enemy" };

            _mockEnemyService.Setup(s => s.UpdateEnemyAsync(enemyId, updateDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.UpdateEnemy(enemyId, updateDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteEnemy_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            _mockEnemyService.Setup(s => s.DeleteEnemyAsync(enemyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteEnemy(enemyId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEnemy_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var enemyId = Guid.NewGuid();
            _mockEnemyService.Setup(s => s.DeleteEnemyAsync(enemyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteEnemy(enemyId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetEnemiesByLocation_ShouldReturnLocationEnemies()
        {
            // Arrange
            var enemies = new List<EnemyDto>
            {
                new EnemyDto { Id = Guid.NewGuid(), Name = "Forest Goblin", Location = "forest" },
                new EnemyDto { Id = Guid.NewGuid(), Name = "Forest Wolf", Location = "forest" }
            };
            _mockEnemyService.Setup(s => s.GetEnemiesByLocationAsync("forest", It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemies);

            // Act
            var result = await _controller.GetEnemiesByLocation("forest");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemies = Assert.IsType<List<EnemyDto>>(okResult.Value);
            Assert.Equal(2, returnedEnemies.Count);
            Assert.All(returnedEnemies, e => Assert.Equal("forest", e.Location));
        }

        [Fact]
        public async Task GetEnemiesByType_ShouldReturnEnemiesOfType()
        {
            // Arrange
            var enemies = new List<EnemyDto>
            {
                new EnemyDto { Id = Guid.NewGuid(), Name = "Goblin Warrior", Type = "Goblin" },
                new EnemyDto { Id = Guid.NewGuid(), Name = "Goblin Archer", Type = "Goblin" }
            };
            _mockEnemyService.Setup(s => s.GetEnemiesByTypeAsync(EnemyType.Goblin, It.IsAny<CancellationToken>()))
                .ReturnsAsync(enemies);

            // Act
            var result = await _controller.GetEnemiesByType(EnemyType.Goblin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEnemies = Assert.IsType<List<EnemyDto>>(okResult.Value);
            Assert.Equal(2, returnedEnemies.Count);
            Assert.All(returnedEnemies, e => Assert.Equal("Goblin", e.Type));
        }

        [Fact]
        public async Task GetEnemiesCountByType_ShouldReturnCounts()
        {
            // Arrange
            var counts = new Dictionary<string, int>
            {
                { "Goblin", 5 },
                { "Dragon", 2 },
                { "Orc", 3 }
            };

            _mockEnemyService.Setup(s => s.GetEnemiesCountByTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(counts);

            // Act
            var result = await _controller.GetEnemiesCountByType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCounts = Assert.IsType<Dictionary<string, int>>(okResult.Value);
            Assert.Equal(5, returnedCounts["Goblin"]);
            Assert.Equal(2, returnedCounts["Dragon"]);
            Assert.Equal(3, returnedCounts["Orc"]);
        }

        [Fact]
        public async Task GetAverageEnemyLevel_ShouldReturnAverage()
        {
            // Arrange
            var averageLevel = 7.5;
            _mockEnemyService.Setup(s => s.GetAverageEnemyLevelAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(averageLevel);

            // Act
            var result = await _controller.GetAverageEnemyLevel();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAverage = Assert.IsType<double>(okResult.Value);
            Assert.Equal(7.5, returnedAverage);
        }

        [Fact]
        public async Task GetTotalGoldReward_ShouldReturnTotalGold()
        {
            // Arrange
            var totalGold = 1250;
            _mockEnemyService.Setup(s => s.GetTotalGoldRewardAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalGold);

            // Act
            var result = await _controller.GetTotalGoldReward();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGold = Assert.IsType<int>(okResult.Value);
            Assert.Equal(1250, returnedGold);
        }

        [Fact]
        public async Task GetEnemiesPaged_ShouldCalculateTotalPagesCorrectly()
        {
            // Arrange
            var pagedResult = new PagedResult<EnemyDto>
            {
                Items = new List<EnemyDto>(),
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 15
                // TotalPages вычисляется автоматически: 15 / 10 = 1.5 → 2
            };

            _mockEnemyService.Setup(s => s.GetEnemiesPagedAsync(
                It.IsAny<PaginationDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetEnemiesPaged(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPagedResult = Assert.IsType<PagedResult<EnemyDto>>(okResult.Value);
            Assert.Equal(2, returnedPagedResult.TotalPages); // Проверяем что вычисляется правильно
            Assert.True(returnedPagedResult.HasNextPage);
            Assert.False(returnedPagedResult.HasPreviousPage);
        }

        [Fact]
public async Task CreateEnemy_WithArgumentException_ShouldReturnBadRequest()
{
    // Arrange
    var createDto = new CreateEnemyDto 
    { 
        Name = "Invalid Enemy",
        Type = EnemyType.Goblin
    };
    
    _mockEnemyService.Setup(s => s.CreateEnemyAsync(createDto, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new ArgumentException("Invalid enemy data"));

    // Act
    var result = await _controller.CreateEnemy(createDto);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    Assert.Equal("Invalid enemy data", badRequestResult.Value);
}

[Fact]
public async Task CreateEnemy_WithGenericException_ShouldReturnInternalServerError()
{
    // Arrange
    var createDto = new CreateEnemyDto 
    { 
        Name = "Test Enemy",
        Type = EnemyType.Goblin
    };
    
    _mockEnemyService.Setup(s => s.CreateEnemyAsync(createDto, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error"));

    // Act
    var result = await _controller.CreateEnemy(createDto);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(500, statusCodeResult.StatusCode);
    Assert.Equal("Internal server error", statusCodeResult.Value);
}

[Fact]
public async Task UpdateEnemy_WithArgumentException_ShouldReturnBadRequest()
{
    // Arrange
    var enemyId = Guid.NewGuid();
    var updateDto = new UpdateEnemyDto { Name = "Invalid Update" };
    
    _mockEnemyService.Setup(s => s.UpdateEnemyAsync(enemyId, updateDto, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new ArgumentException("Invalid update data"));

    // Act
    var result = await _controller.UpdateEnemy(enemyId, updateDto);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    Assert.Equal("Invalid update data", badRequestResult.Value);
}

[Fact]
public async Task UpdateEnemy_WithGenericException_ShouldReturnInternalServerError()
{
    // Arrange
    var enemyId = Guid.NewGuid();
    var updateDto = new UpdateEnemyDto { Name = "Updated Enemy" };
    
    _mockEnemyService.Setup(s => s.UpdateEnemyAsync(enemyId, updateDto, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Update failed"));

    // Act
    var result = await _controller.UpdateEnemy(enemyId, updateDto);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(500, statusCodeResult.StatusCode);
    Assert.Equal("Internal server error", statusCodeResult.Value);
}

[Fact]
public async Task DeleteEnemy_WithGenericException_ShouldReturnInternalServerError()
{
    // Arrange
    var enemyId = Guid.NewGuid();
    
    _mockEnemyService.Setup(s => s.DeleteEnemyAsync(enemyId, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Delete failed"));

    // Act
    var result = await _controller.DeleteEnemy(enemyId);

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(500, statusCodeResult.StatusCode);
    Assert.Equal("Internal server error", statusCodeResult.Value);
}

[Fact]
public async Task GetEnemiesPaged_WithDefaultParameters_ShouldUseDefaults()
{
    // Arrange
    var pagedResult = new PagedResult<EnemyDto>
    {
        Items = new List<EnemyDto>(),
        PageNumber = 1,
        PageSize = 10,
        TotalCount = 0
    };

    _mockEnemyService.Setup(s => s.GetEnemiesPagedAsync(
        It.Is<PaginationDto>(p => 
            p.Page == 1 && 
            p.PageSize == 10 && 
            p.SortBy == null && 
            p.SortOrder == "asc" &&
            p.Search == null), 
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(pagedResult);

    // Act - используем все параметры по умолчанию
    var result = await _controller.GetEnemiesPaged();

    // Assert
    Assert.IsType<OkObjectResult>(result.Result);
}

[Fact]
public async Task GetEnemiesByLevelRange_WithDefaultParameters_ShouldUseDefaults()
{
    // Arrange
    var enemies = new List<EnemyDto>();
    _mockEnemyService.Setup(s => s.GetEnemiesByLevelRangeAsync(1, 100, It.IsAny<CancellationToken>()))
        .ReturnsAsync(enemies);

    // Act - используем параметры по умолчанию
    var result = await _controller.GetEnemiesByLevelRange();

    // Assert
    Assert.IsType<OkObjectResult>(result.Result);
}

[Fact]
public async Task GetEnemiesByRewardRange_WithDefaultParameters_ShouldUseDefaults()
{
    // Arrange
    var enemies = new List<EnemyDto>();
    _mockEnemyService.Setup(s => s.GetEnemiesByRewardRangeAsync(0, 10000, 0, 10000, It.IsAny<CancellationToken>()))
        .ReturnsAsync(enemies);

    // Act - используем параметры по умолчанию
    var result = await _controller.GetEnemiesByRewardRange();

    // Assert
    Assert.IsType<OkObjectResult>(result.Result);
}
    }
}