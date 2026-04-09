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
    public class PlayersControllerTests
    {
        private readonly Mock<IPlayerService> _mockPlayerService;
        private readonly Mock<ILogger<PlayersController>> _mockLogger;
        private readonly PlayersController _controller;

        public PlayersControllerTests()
        {
            _mockPlayerService = new Mock<IPlayerService>();
            _mockLogger = new Mock<ILogger<PlayersController>>();
            _controller = new PlayersController(_mockPlayerService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetPlayer_WithValidId_ShouldReturnPlayer()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var playerDto = new PlayerDto { Id = playerId, Name = "TestPlayer" };
            _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(playerDto);

            // Act
            var result = await _controller.GetPlayer(playerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayer = Assert.IsType<PlayerDto>(okResult.Value);
            Assert.Equal(playerId, returnedPlayer.Id);
            Assert.Equal("TestPlayer", returnedPlayer.Name);
        }

        [Fact]
        public async Task GetPlayer_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Player not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetPlayer(playerId));
        }

        [Fact]
        public async Task CreatePlayer_WithValidData_ShouldReturnCreatedPlayer()
        {
            // Arrange
            var createDto = new CreatePlayerDto { Name = "NewPlayer", Email = "new@example.com" };
            var playerDto = new PlayerDto { Id = Guid.NewGuid(), Name = "NewPlayer", Email = "new@example.com" };

            _mockPlayerService.Setup(s => s.CreatePlayerAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(playerDto);

            // Act
            var result = await _controller.CreatePlayer(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedPlayer = Assert.IsType<PlayerDto>(createdResult.Value);
            Assert.Equal("NewPlayer", returnedPlayer.Name);
            Assert.Equal("GetPlayer", createdResult.ActionName);
        }

        [Fact]
        public async Task CreatePlayer_WithDuplicateEmail_ShouldThrowConflictException()
        {
            // Arrange
            var createDto = new CreatePlayerDto { Name = "NewPlayer", Email = "duplicate@example.com" };
            _mockPlayerService.Setup(s => s.CreatePlayerAsync(createDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictException("Player with this email already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.CreatePlayer(createDto));
        }

        [Fact]
        public async Task UpdatePlayer_WithValidData_ShouldReturnUpdatedPlayer()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var updateDto = new UpdatePlayerDto { Name = "UpdatedPlayer" };
            var playerDto = new PlayerDto { Id = playerId, Name = "UpdatedPlayer" };

            _mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(playerDto);

            // Act
            var result = await _controller.UpdatePlayer(playerId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayer = Assert.IsType<PlayerDto>(okResult.Value);
            Assert.Equal("UpdatedPlayer", returnedPlayer.Name);
        }

        [Fact]
        public async Task UpdatePlayer_WithDuplicateEmail_ShouldThrowConflictException()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var updateDto = new UpdatePlayerDto { Email = "duplicate@example.com" };

            _mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, updateDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictException("Player with this email already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.UpdatePlayer(playerId, updateDto));
        }

        [Fact]
        public async Task CompleteQuestWithTransaction_WithValidIds_ShouldReturnPlayer()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var questId = Guid.NewGuid();
            var playerDto = new PlayerDto { Id = playerId, Name = "Player", Experience = 150 };
            _mockPlayerService.Setup(s => s.CompleteQuestWithTransactionAsync(playerId, questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(playerDto);

            // Act
            var result = await _controller.CompleteQuestWithTransaction(playerId, questId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayer = Assert.IsType<PlayerDto>(okResult.Value);
            Assert.Equal(150, returnedPlayer.Experience);
        }

        [Fact]
        public async Task GetAlivePlayers_ShouldReturnAlivePlayers()
        {
            // Arrange
            var players = new List<PlayerDto> 
            { 
                new PlayerDto { Id = Guid.NewGuid(), Name = "Alive", Health = 50 },
                new PlayerDto { Id = Guid.NewGuid(), Name = "Dead", Health = 0 }
            };
            _mockPlayerService.Setup(s => s.GetAlivePlayersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(players.Where(p => p.Health > 0).ToList());

            // Act
            var result = await _controller.GetAlivePlayers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
            Assert.Single(returnedPlayers);
            Assert.Equal("Alive", returnedPlayers.First().Name);
        }

        [Fact]
        public async Task SearchPlayers_WithEmptyTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchPlayers("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task DeletePlayer_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            _mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePlayer(playerId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePlayer_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            _mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeletePlayer(playerId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetPlayersCountByLevel_ShouldReturnCounts()
        {
            // Arrange
            var counts = new Dictionary<int, int>
            {
                { 1, 5 },
                { 2, 3 },
                { 3, 1 }
            };
            
            _mockPlayerService.Setup(s => s.GetPlayersCountByLevelAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(counts);

            // Act
            var result = await _controller.GetPlayersCountByLevel();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCounts = Assert.IsType<Dictionary<int, int>>(okResult.Value);
            Assert.Equal(3, returnedCounts.Count);
            Assert.Equal(5, returnedCounts[1]);
            Assert.Equal(3, returnedCounts[2]);
            Assert.Equal(1, returnedCounts[3]);
        }

        [Fact]
        public async Task GetTotalPlayerGold_ShouldReturnTotalGold()
        {
            // Arrange
            var totalGold = 5000;
            _mockPlayerService.Setup(s => s.GetTotalPlayerGoldAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalGold);

            // Act
            var result = await _controller.GetTotalPlayerGold();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(totalGold, okResult.Value);
        }

        [Fact]
        public async Task GetPlayerStats_ShouldReturnStats()
        {
            // Arrange
            var stats = new PlayerStatsDto
            {
                TotalPlayers = 10,
                AverageLevel = 2.5,
                TotalGold = 5000,
                MaxLevel = 5,           
                MinLevel = 1  
            };
            
            _mockPlayerService.Setup(s => s.GetPlayerStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            // Act
            var result = await _controller.GetPlayerStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStats = Assert.IsType<PlayerStatsDto>(okResult.Value);
            Assert.Equal(10, returnedStats.TotalPlayers);
            Assert.Equal(2.5, returnedStats.AverageLevel);
            Assert.Equal(5000, returnedStats.TotalGold);
        }

        [Fact]
        public async Task GetPlayersPaged_ShouldReturnPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<PlayerDto>
            {
                Items = new List<PlayerDto> 
                { 
                    new PlayerDto { Id = Guid.NewGuid(), Name = "Player1" },
                    new PlayerDto { Id = Guid.NewGuid(), Name = "Player2" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 20
            };

            _mockPlayerService.Setup(s => s.GetPlayersPagedAsync(
                It.IsAny<PaginationDto>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPlayersPaged();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<PlayerDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.Items.Count());
            Assert.Equal(1, returnedResult.PageNumber);
            Assert.Equal(20, returnedResult.TotalCount);
        }

        [Fact]
        public async Task GetPlayersByLevelRange_ShouldReturnPlayersInRange()
        {
            // Arrange
            var players = new List<PlayerDto>
            {
                new PlayerDto { Id = Guid.NewGuid(), Name = "LowLevel", Level = 2 },
                new PlayerDto { Id = Guid.NewGuid(), Name = "MidLevel", Level = 5 },
                new PlayerDto { Id = Guid.NewGuid(), Name = "HighLevel", Level = 15 }
            };
            
            _mockPlayerService.Setup(s => s.GetPlayersByLevelRangeAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(players.Where(p => p.Level <= 10).ToList());

            // Act
            var result = await _controller.GetPlayersByLevelRange(minLevel: 1, maxLevel: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
            Assert.Equal(2, returnedPlayers.Count);
            Assert.Contains(returnedPlayers, p => p.Name == "LowLevel");
            Assert.Contains(returnedPlayers, p => p.Name == "MidLevel");
            Assert.DoesNotContain(returnedPlayers, p => p.Name == "HighLevel");
        }

        [Fact]
public async Task SearchPlayers_WithValidTerm_ShouldReturnMatchingPlayers()
{
    // Arrange
    var players = new List<PlayerDto> 
    { 
        new PlayerDto { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" },
        new PlayerDto { Id = Guid.NewGuid(), Name = "Jane Doe", Email = "jane@example.com" }
    };
    
    _mockPlayerService.Setup(s => s.SearchPlayersAsync("doe", It.IsAny<CancellationToken>()))
        .ReturnsAsync(players);

    // Act
    var result = await _controller.SearchPlayers("doe");

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
    Assert.Equal(2, returnedPlayers.Count);
}

[Fact]
public async Task SearchPlayers_WithNoResults_ShouldReturnEmptyList()
{
    // Arrange
    var emptyList = new List<PlayerDto>();
    _mockPlayerService.Setup(s => s.SearchPlayersAsync("nonexistent", It.IsAny<CancellationToken>()))
        .ReturnsAsync(emptyList);

    // Act
    var result = await _controller.SearchPlayers("nonexistent");

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
    Assert.Empty(returnedPlayers);
}

[Fact]
public async Task SearchPlayers_WithWhitespaceTerm_ShouldReturnBadRequest()
{
    // Act
    var result = await _controller.SearchPlayers("   ");

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    Assert.Equal("Search term is required", badRequestResult.Value);
}
    }
}