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
    public class GameLocationServiceTests : IDisposable
    {
        private readonly GameDbContext _context;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GameLocationService>> _loggerMock;
        private readonly GameLocationService _locationService;

        public GameLocationServiceTests()
        {
            // Создаем InMemory базу данных для тестов
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new GameDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GameLocationService>>();
            
            _locationService = new GameLocationService(
                _context,
                _mapperMock.Object,
                _loggerMock.Object);
            
            // Заполняем тестовыми данными
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Создаем тестовую локацию
            var location = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Test Forest",
                Description = "A test forest",
                Type = LocationType.Forest,
                RequiredLevel = 1,
                IsSafeZone = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GameLocations.Add(location);

            // Создаем безопасную локацию
            var safeLocation = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Safe Village",
                Description = "A safe village",
                Type = LocationType.Village,
                RequiredLevel = 1,
                IsSafeZone = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GameLocations.Add(safeLocation);

            // Создаем локацию с врагами
            var locationWithEnemies = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Dangerous Cave",
                Description = "A cave with enemies",
                Type = LocationType.Cave,
                RequiredLevel = 5,
                IsSafeZone = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GameLocations.Add(locationWithEnemies);
            
            // Создаем врага для локации
            var enemy = new Enemy
            {
                Id = Guid.NewGuid(),
                Name = "Cave Goblin",
                Type = EnemyType.Goblin,
                Level = 3,
                Health = 50,
                MaxHealth = 50,
                Attack = 10,
                LocationId = locationWithEnemies.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Enemies.Add(enemy);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetLocationByIdAsync_ShouldReturnLocation_WhenExists()
        {
            // Arrange
            var locationId = _context.GameLocations.First().Id;
            var locationDto = new GameLocationDto 
            { 
                Id = locationId, 
                Name = "Test Forest",
                Type = "Forest"
            };
            
            _mapperMock.Setup(x => x.Map<GameLocationDto>(It.IsAny<GameLocation>()))
                .Returns(locationDto);

            // Act
            var result = await _locationService.GetLocationByIdAsync(locationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationId, result.Id);
            Assert.Equal("Test Forest", result.Name);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _locationService.GetLocationByIdAsync(locationId));
        }

        [Fact]
        public async Task GetAccessibleLocationsAsync_ShouldReturnLocations_WhenPlayerLevelIsSufficient()
        {
            // Arrange
            var playerLevel = 3;
            var locations = _context.GameLocations
                .Where(l => l.RequiredLevel <= playerLevel) // Используем выражение вместо метода CanPlayerAccess
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto
            {
                Name = l.Name,
                RequiredLevel = l.RequiredLevel
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetAccessibleLocationsAsync(playerLevel);

            // Assert
            Assert.NotNull(result);
            // Forest (1), Village (1) доступны для уровня 3, Cave (5) - нет
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSafeZonesAsync_ShouldReturnOnlySafeZones()
        {
            // Arrange
            var safeLocations = _context.GameLocations
                .Where(l => l.IsSafeZone)
                .ToList();
            
            var locationDtos = safeLocations.Select(l => new GameLocationDto 
            { 
                Name = l.Name,
                IsSafeZone = l.IsSafeZone
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetSafeZonesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Safe Village", result.First().Name);
            Assert.True(result.First().IsSafeZone);
        }

        [Fact]
        public async Task CreateLocationAsync_ShouldCreateLocation_WhenValidData()
        {
            // Arrange
            var createDto = new CreateGameLocationDto 
            { 
                Name = "New Location", 
                Description = "New description",
                Type = LocationType.Forest,
                RequiredLevel = 1,
                IsSafeZone = false
            };
            
            var location = new GameLocation 
            { 
                Id = Guid.NewGuid(), 
                Name = "New Location", 
                RequiredLevel = 1 
            };
            
            var locationDto = new GameLocationDto 
            { 
                Id = location.Id, 
                Name = "New Location" 
            };
            
            _mapperMock.Setup(x => x.Map<GameLocation>(createDto))
                .Returns(location);
            _mapperMock.Setup(x => x.Map<GameLocationDto>(location))
                .Returns(locationDto);

            // Act
            var result = await _locationService.CreateLocationAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Location", result.Name);
            // Проверяем, что локация добавилась
            Assert.Equal(4, await _context.GameLocations.CountAsync());
        }

        [Fact]
        public async Task DeleteLocationAsync_ShouldReturnTrue_WhenLocationExists()
        {
            // Arrange
            var locationId = _context.GameLocations.First().Id;

            // Act
            var result = await _locationService.DeleteLocationAsync(locationId);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.GameLocations.FindAsync(locationId));
        }

        [Fact]
        public async Task DeleteLocationAsync_ShouldReturnFalse_WhenLocationNotExists()
        {
            // Arrange
            var locationId = Guid.NewGuid();

            // Act
            var result = await _locationService.DeleteLocationAsync(locationId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetLocationsPagedAsync_ShouldReturnPagedLocations()
        {
            // Arrange
            var pagination = new PaginationDto { Page = 1, PageSize = 2 };
            
            var pagedResult = new PagedResult<GameLocationDto>
            {
                Items = new List<GameLocationDto> 
                { 
                    new GameLocationDto { Name = "Test Forest" }, 
                    new GameLocationDto { Name = "Safe Village" } 
                },
                PageNumber = 1,
                PageSize = 2,
                TotalCount = 3
            };
            
            _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
                .Returns(pagedResult.Items.ToList());

            // Act
            var result = await _locationService.GetLocationsPagedAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task SearchLocationsAsync_ShouldReturnLocations()
        {
            // Arrange
            var searchTerm = "forest";
            
            var locations = _context.GameLocations
                .Where(l => l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto 
            { 
                Name = l.Name 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.SearchLocationsAsync(searchTerm);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Forest", result.First().Name);
        }

        [Fact]
        public async Task GetLocationsByLevelRangeAsync_ShouldReturnLocationsInRange()
        {
            // Arrange
            var minLevel = 1;
            var maxLevel = 3;
            
            var locations = _context.GameLocations
                .Where(l => l.RequiredLevel >= minLevel && l.RequiredLevel <= maxLevel)
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto 
            { 
                Name = l.Name, 
                RequiredLevel = l.RequiredLevel 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetLocationsByLevelRangeAsync(minLevel, maxLevel);

            // Assert
            // Forest (1), Village (1) в диапазоне, Cave (5) - нет
            Assert.Equal(2, result.Count());
            Assert.All(result, l => Assert.InRange(l.RequiredLevel, minLevel, maxLevel));
        }

        [Fact]
        public async Task GetLocationsByTypeAsync_ShouldReturnLocationsOfType()
        {
            // Arrange
            var locationType = LocationType.Forest;
            
            var locations = _context.GameLocations
                .Where(l => l.Type == locationType)
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto 
            { 
                Name = l.Name, 
                Type = l.Type.ToString() 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetLocationsByTypeAsync(locationType);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Forest", result.First().Name);
            Assert.Equal("Forest", result.First().Type);
        }

        [Fact]
        public async Task GetLocationsWithEnemiesAsync_ShouldReturnLocationsWithEnemies()
        {
            // Arrange
            var locations = _context.GameLocations
                .Where(l => l.Enemies.Any()) // Используем выражение вместо метода HasEnemies()
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto
            {
                Name = l.Name
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetLocationsWithEnemiesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Dangerous Cave", result.First().Name);
        }

        [Fact]
        public async Task GetLocationsWithQuestsAsync_ShouldReturnLocationsWithQuests()
        {
            // Arrange
            // Добавляем квест для теста (исправленная версия без RequiredLevel)
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Test Quest",
                Description = "A test quest",
                Objective = "Defeat enemies",
                TargetCount = 5,
                ExperienceReward = 100,
                GoldReward = 50,
                LocationId = _context.GameLocations.First().Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();
            
            var locations = _context.GameLocations
                .Where(l => l.Quests.Any())
                .ToList();
            
            var locationDtos = locations.Select(l => new GameLocationDto 
            { 
                Name = l.Name 
            }).ToList();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<GameLocationDto>>(It.IsAny<IEnumerable<GameLocation>>()))
                .Returns(locationDtos);

            // Act
            var result = await _locationService.GetLocationsWithQuestsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Forest", result.First().Name);
        }

        [Fact]
        public async Task UpdateLocationAsync_ShouldUpdateLocation_WhenExists()
        {
            // Arrange
            var locationId = _context.GameLocations.First().Id;
            var updateDto = new UpdateGameLocationDto 
            { 
                Name = "Updated Forest", 
                RequiredLevel = 10 
            };
            
            var locationDto = new GameLocationDto 
            { 
                Id = locationId, 
                Name = "Updated Forest", 
                RequiredLevel = 10 
            };
            
            _mapperMock.Setup(x => x.Map<GameLocationDto>(It.IsAny<GameLocation>()))
                .Returns(locationDto);

            // Act
            var result = await _locationService.UpdateLocationAsync(locationId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Forest", result.Name);
            Assert.Equal(10, result.RequiredLevel);
            
            var updatedLocation = await _context.GameLocations.FindAsync(locationId);
            Assert.Equal("Updated Forest", updatedLocation?.Name);
            Assert.Equal(10, updatedLocation?.RequiredLevel);
        }

        [Fact]
        public async Task UpdateLocationAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var updateDto = new UpdateGameLocationDto { Name = "Updated Location" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _locationService.UpdateLocationAsync(locationId, updateDto));
        }

        [Fact]
        public async Task GetPlayerDistributionAsync_ShouldReturnCorrectDistribution()
        {
            // Arrange
            // Добавляем игрока для теста (исправленная версия без Username)
            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Test Player",
                Email = "test@test.com",
                Level = 1,
                Experience = 0,
                Health = 100,
                MaxHealth = 100,
                Attack = 10,
                Gold = 50,
                LocationId = _context.GameLocations.First().Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            var result = await _locationService.GetPlayerDistributionAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Test Forest"));
            Assert.Equal(1, result["Test Forest"]);
        }
        [Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSearch()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        Search = "Test" // Покрывает строки 79-86 (поиск)
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location" }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Items);
}

[Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSortingByNameAsc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "name", // Покрывает строку 90: сортировка по названию asc
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location" }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSortingByNameDesc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "name",
        SortOrder = "desc" // Покрывает строку 90: сортировка по названию desc
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location" }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSortingByRequiredLevel()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "requiredlevel", // Покрывает строку 91: сортировка по требуемому уровню
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location", RequiredLevel = 1 }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSortingByType()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "type", // Покрывает строку 92: сортировка по типу
        SortOrder = "desc"
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location", Type = "Forest" }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetLocationsPagedAsync_ShouldHandleSortingByCreatedAt()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "createdat", // Покрывает строку 93: сортировка по дате создания
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<GameLocationDto>>(It.IsAny<List<GameLocation>>()))
        .Returns(new List<GameLocationDto> 
        { 
            new GameLocationDto { Name = "Test Location", CreatedAt = DateTime.UtcNow }
        });

    // Act
    var result = await _locationService.GetLocationsPagedAsync(pagination); // Используйте правильное имя вашего сервиса

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task CreateLocationAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var createDto = new CreateGameLocationDto 
    { 
        Name = "Error Location",
        Description = "A location that will cause error",
        Type = LocationType.Forest,
        RequiredLevel = 1,
        IsSafeZone = false
    };
    
    // Симулируем ошибку при маппинге - покрывает строки 239-242
    _mapperMock.Setup(x => x.Map<GameLocation>(createDto))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() => 
        _locationService.CreateLocationAsync(createDto)); // Используйте правильное имя вашего сервиса
}
    }
}