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
    public class GameLocationsControllerTests
    {
        private readonly Mock<IGameLocationService> _mockLocationService;
        private readonly Mock<ILogger<GameLocationsController>> _mockLogger;
        private readonly GameLocationsController _controller;

        public GameLocationsControllerTests()
        {
            _mockLocationService = new Mock<IGameLocationService>();
            _mockLogger = new Mock<ILogger<GameLocationsController>>();
            _controller = new GameLocationsController(_mockLocationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLocation_WithValidId_ShouldReturnLocation()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var locationDto = new GameLocationDto 
            { 
                Id = locationId, 
                Name = "Dark Forest", 
                Description = "A mysterious dark forest",
                Type = "Forest",
                RequiredLevel = 5
            };
            _mockLocationService.Setup(s => s.GetLocationByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locationDto);

            // Act
            var result = await _controller.GetLocation(locationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocation = Assert.IsType<GameLocationDto>(okResult.Value);
            Assert.Equal(locationId, returnedLocation.Id);
            Assert.Equal("Dark Forest", returnedLocation.Name);
            Assert.Equal("A mysterious dark forest", returnedLocation.Description);
            Assert.Equal("Forest", returnedLocation.Type);
            Assert.Equal(5, returnedLocation.RequiredLevel);
        }

        [Fact]
        public async Task GetLocation_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationService.Setup(s => s.GetLocationByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Location not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetLocation(locationId));
        }

        [Fact]
        public async Task GetLocationsPaged_ShouldReturnAllLocations_WhenNoParameters()
        {
            // Arrange
            var pagedResult = new PagedResult<GameLocationDto>
            {
                Items = new List<GameLocationDto> 
                { 
                    new GameLocationDto { Id = Guid.NewGuid(), Name = "Forest", Type = "Forest" },
                    new GameLocationDto { Id = Guid.NewGuid(), Name = "Cave", Type = "Cave" },
                    new GameLocationDto { Id = Guid.NewGuid(), Name = "Village", Type = "Village" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 3
            };

            _mockLocationService.Setup(s => s.GetLocationsPagedAsync(
                It.Is<PaginationDto>(p => 
                    p.Page == 1 && 
                    p.PageSize == 10 && 
                    p.SortBy == null && 
                    p.SortOrder == "asc" &&
                    p.Search == null), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetLocationsPaged();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPagedResult = Assert.IsType<PagedResult<GameLocationDto>>(okResult.Value);
            Assert.Equal(3, returnedPagedResult.Items.Count());
            Assert.Contains(returnedPagedResult.Items, l => l.Name == "Forest");
            Assert.Contains(returnedPagedResult.Items, l => l.Name == "Cave");
            Assert.Contains(returnedPagedResult.Items, l => l.Name == "Village");
        }

        [Fact]
        public async Task GetLocationsPaged_WithValidParameters_ShouldReturnPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<GameLocationDto>
            {
                Items = new List<GameLocationDto> 
                { 
                    new GameLocationDto { Id = Guid.NewGuid(), Name = "Forest", RequiredLevel = 3 }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 25
            };

            _mockLocationService.Setup(s => s.GetLocationsPagedAsync(
                It.Is<PaginationDto>(p => 
                    p.Page == 1 && 
                    p.PageSize == 10 && 
                    p.SortBy == "name" && 
                    p.SortOrder == "asc" &&
                    p.Search == "forest"), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetLocationsPaged(
                page: 1, 
                pageSize: 10, 
                sortBy: "name", 
                sortOrder: "asc", 
                search: "forest");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPagedResult = Assert.IsType<PagedResult<GameLocationDto>>(okResult.Value);
            Assert.Single(returnedPagedResult.Items);
            Assert.Equal(1, returnedPagedResult.PageNumber);
            Assert.Equal(10, returnedPagedResult.PageSize);
            Assert.Equal(25, returnedPagedResult.TotalCount);
        }

        [Fact]
        public async Task SearchLocations_WithValidTerm_ShouldReturnMatchingLocations()
        {
            // Arrange
            var locations = new List<GameLocationDto> 
            { 
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Dark Forest", Description = "A scary forest" }
            };
            _mockLocationService.Setup(s => s.SearchLocationsAsync("forest", It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations);

            // Act
            var result = await _controller.SearchLocations("forest");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocations = Assert.IsType<List<GameLocationDto>>(okResult.Value);
            Assert.Single(returnedLocations);
            Assert.Equal("Dark Forest", returnedLocations.First().Name);
        }

        [Fact]
        public async Task SearchLocations_WithEmptyTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchLocations("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchLocations_WithWhitespaceTerm_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.SearchLocations("   ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetLocationsByLevelRange_ShouldReturnLocationsInRange()
        {
            // Arrange
            var locations = new List<GameLocationDto>
            {
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Easy Forest", RequiredLevel = 2 },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Medium Cave", RequiredLevel = 7 },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Hard Mountain", RequiredLevel = 15 }
            };
            _mockLocationService.Setup(s => s.GetLocationsByLevelRangeAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations.Where(l => l.RequiredLevel <= 10).ToList());

            // Act
            var result = await _controller.GetLocationsByLevelRange(minLevel: 1, maxLevel: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocations = Assert.IsType<List<GameLocationDto>>(okResult.Value);
            Assert.Equal(2, returnedLocations.Count);
            Assert.Contains(returnedLocations, l => l.Name == "Easy Forest");
            Assert.Contains(returnedLocations, l => l.Name == "Medium Cave");
            Assert.DoesNotContain(returnedLocations, l => l.Name == "Hard Mountain");
        }

        [Fact]
        public async Task CreateLocation_WithValidData_ShouldReturnCreatedLocation()
        {
            // Arrange
            var createDto = new CreateGameLocationDto 
            { 
                Name = "New Cave",
                Description = "A newly discovered cave system",
                Type = LocationType.Cave,
                RequiredLevel = 8,
                IsSafeZone = false
            };
            var locationDto = new GameLocationDto 
            { 
                Id = Guid.NewGuid(), 
                Name = "New Cave",
                Description = "A newly discovered cave system",
                Type = "Cave",
                RequiredLevel = 8,
                IsSafeZone = false
            };

            _mockLocationService.Setup(s => s.CreateLocationAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locationDto);

            // Act
            var result = await _controller.CreateLocation(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(GameLocationsController.GetLocation), createdResult.ActionName);
            Assert.Equal(locationDto.Id, createdResult.RouteValues?["id"]);
            
            var returnedLocation = Assert.IsType<GameLocationDto>(createdResult.Value);
            Assert.Equal("New Cave", returnedLocation.Name);
            Assert.Equal("Cave", returnedLocation.Type);
            Assert.Equal(8, returnedLocation.RequiredLevel);
            Assert.False(returnedLocation.IsSafeZone);
        }

        [Fact]
        public async Task UpdateLocation_WithValidData_ShouldReturnUpdatedLocation()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var updateDto = new UpdateGameLocationDto 
            { 
                Name = "Updated Forest",
                Description = "An updated description",
                RequiredLevel = 12,
                IsSafeZone = true
            };
            var locationDto = new GameLocationDto 
            { 
                Id = locationId, 
                Name = "Updated Forest",
                Description = "An updated description",
                RequiredLevel = 12,
                IsSafeZone = true
            };

            _mockLocationService.Setup(s => s.UpdateLocationAsync(locationId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locationDto);

            // Act
            var result = await _controller.UpdateLocation(locationId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocation = Assert.IsType<GameLocationDto>(okResult.Value);
            Assert.Equal("Updated Forest", returnedLocation.Name);
            Assert.Equal("An updated description", returnedLocation.Description);
            Assert.Equal(12, returnedLocation.RequiredLevel);
            Assert.True(returnedLocation.IsSafeZone);
        }

        [Fact]
        public async Task UpdateLocation_WithInvalidId_ShouldThrowNotFoundException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var updateDto = new UpdateGameLocationDto { Name = "Updated Location" };

            _mockLocationService.Setup(s => s.UpdateLocationAsync(locationId, updateDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Location not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _controller.UpdateLocation(locationId, updateDto));
        }

        [Fact]
        public async Task DeleteLocation_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationService.Setup(s => s.DeleteLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteLocation(locationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteLocation_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationService.Setup(s => s.DeleteLocationAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteLocation(locationId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAccessibleLocations_ShouldReturnAccessibleLocationsForPlayerLevel()
        {
            // Arrange
            var locations = new List<GameLocationDto>
            {
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Easy Forest", RequiredLevel = 3 },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Medium Cave", RequiredLevel = 8 },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Hard Mountain", RequiredLevel = 15 }
            };
            _mockLocationService.Setup(s => s.GetAccessibleLocationsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations.Where(l => l.RequiredLevel <= 10).ToList());

            // Act
            var result = await _controller.GetAccessibleLocations(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocations = Assert.IsType<List<GameLocationDto>>(okResult.Value);
            Assert.Equal(2, returnedLocations.Count);
            Assert.Contains(returnedLocations, l => l.Name == "Easy Forest");
            Assert.Contains(returnedLocations, l => l.Name == "Medium Cave");
            Assert.DoesNotContain(returnedLocations, l => l.Name == "Hard Mountain");
        }

        [Fact]
        public async Task GetLocationsByType_ShouldReturnLocationsOfSpecificType()
        {
            // Arrange
            var locations = new List<GameLocationDto>
            {
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Dark Forest", Type = "Forest" },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Ancient Forest", Type = "Forest" },
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Deep Cave", Type = "Cave" }
            };
            _mockLocationService.Setup(s => s.GetLocationsByTypeAsync(LocationType.Forest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations.Where(l => l.Type == "Forest").ToList());

            // Act
            var result = await _controller.GetLocationsByType(LocationType.Forest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocations = Assert.IsType<List<GameLocationDto>>(okResult.Value);
            Assert.Equal(2, returnedLocations.Count);
            Assert.All(returnedLocations, l => Assert.Equal("Forest", l.Type));
            Assert.Contains(returnedLocations, l => l.Name == "Dark Forest");
            Assert.Contains(returnedLocations, l => l.Name == "Ancient Forest");
            Assert.DoesNotContain(returnedLocations, l => l.Name == "Deep Cave");
        }

        [Fact]
        public async Task GetPlayerDistribution_ShouldReturnPlayerCountsByLocation()
        {
            // Arrange
            var distribution = new Dictionary<string, int>
            {
                { "Forest", 5 },
                { "Cave", 3 },
                { "Village", 8 }
            };
            
            _mockLocationService.Setup(s => s.GetPlayerDistributionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(distribution);

            // Act
            var result = await _controller.GetPlayerDistribution();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDistribution = Assert.IsType<Dictionary<string, int>>(okResult.Value);
            Assert.Equal(3, returnedDistribution.Count);
            Assert.Equal(5, returnedDistribution["Forest"]);
            Assert.Equal(3, returnedDistribution["Cave"]);
            Assert.Equal(8, returnedDistribution["Village"]);
        }

        [Fact]
        public async Task GetLocationsByLevelRange_WithDefaultParameters_ShouldUseDefaultValues()
        {
            // Arrange
            var locations = new List<GameLocationDto>
            {
                new GameLocationDto { Id = Guid.NewGuid(), Name = "Location", RequiredLevel = 5 }
            };
            _mockLocationService.Setup(s => s.GetLocationsByLevelRangeAsync(1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations);

            // Act
            var result = await _controller.GetLocationsByLevelRange();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedLocations = Assert.IsType<List<GameLocationDto>>(okResult.Value);
            Assert.Single(returnedLocations);
        }

        // Убраны тесты для методов, которых нет в контроллере:
        // - GetSafeZones
        // - GetLocationsWithEnemies  
        // - GetLocationsWithQuests
    }
}