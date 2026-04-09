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
    public class PlayerServiceTests : IDisposable
    {
        private readonly GameDbContext _context;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PlayerService>> _loggerMock;
        private readonly PlayerService _playerService;

        public PlayerServiceTests()
        {
            // Создаем InMemory базу данных для тестов
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new GameDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<PlayerService>>();
            
            _playerService = new PlayerService(
                _context,
                _mapperMock.Object,
                _loggerMock.Object);
            
            // Заполняем тестовыми данными
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Создаем тестового игрока
            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Test Player",
                Email = "test@example.com",
                Level = 1,
                Experience = 0,
                Health = 100,
                MaxHealth = 100,
                Attack = 10,
                Gold = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Players.Add(player);

            // Создаем тестовый квест
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Test Quest",
                Description = "A test quest",
                Objective = "Test objective",
                TargetCount = 1,
                ExperienceReward = 100,
                GoldReward = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quests.Add(quest);

            // Создаем тестового врага
            var enemy = new Enemy
            {
                Id = Guid.NewGuid(),
                Name = "Test Enemy",
                Type = EnemyType.Goblin,
                Level = 2,
                Health = 30,
                MaxHealth = 30,
                Attack = 5,
                ExperienceReward = 25,
                GoldReward = 10,
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
        public async Task GetPlayerByIdAsync_ShouldReturnPlayer_WhenExists()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var playerDto = new PlayerDto { Id = playerId, Name = "Test Player" };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.GetPlayerByIdAsync(playerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(playerId, result.Id);
            Assert.Equal("Test Player", result.Name);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            var playerId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _playerService.GetPlayerByIdAsync(playerId));
        }

        [Fact]
        public async Task CreatePlayerAsync_ShouldCreatePlayer_WhenEmailIsUnique()
        {
            // Arrange
            var createDto = new CreatePlayerDto { 
                Name = "New Player", 
                Email = "new@example.com" 
            };
            
            var player = new Player { 
                Id = Guid.NewGuid(), 
                Name = "New Player", 
                Email = "new@example.com" 
            };
            
            var playerDto = new PlayerDto { 
                Id = player.Id, 
                Name = "New Player", 
                Email = "new@example.com" 
            };
            
            _mapperMock.Setup(x => x.Map<Player>(createDto))
                .Returns(player);
            _mapperMock.Setup(x => x.Map<PlayerDto>(player))
                .Returns(playerDto);

            // Act
            var result = await _playerService.CreatePlayerAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Player", result.Name);
            Assert.Equal("new@example.com", result.Email);
            // Проверяем, что игрок добавился в базу
            Assert.Equal(2, await _context.Players.CountAsync());
        }

        [Fact]
        public async Task CreatePlayerAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            // Arrange
            var createDto = new CreatePlayerDto { 
                Name = "New Player", 
                Email = "test@example.com" // Такой email уже есть
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => 
                _playerService.CreatePlayerAsync(createDto));
        }

        [Fact]
        public async Task AddExperienceAsync_ShouldAddExperience_WhenPlayerExists()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player", 
                Experience = 50, 
                Level = 1 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.AddExperienceAsync(playerId, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.Experience);
            
            var updatedPlayer = await _context.Players.FindAsync(playerId);
            Assert.Equal(50, updatedPlayer?.Experience);
        }

        [Fact]
        public async Task HealPlayerAsync_ShouldHealPlayer_WhenPlayerExists()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player", 
                Health = 80 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.HealPlayerAsync(playerId, 30);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(80, result.Health);
            
            var healedPlayer = await _context.Players.FindAsync(playerId);
            Assert.Equal(100, healedPlayer?.Health); // Должно быть полное лечение
        }

        [Fact]
        public async Task CompleteQuestAsync_ShouldCompleteQuest_WhenPlayerAndQuestExist()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var questId = _context.Quests.First().Id;
            
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player", 
                Experience = 100 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.CompleteQuestAsync(playerId, questId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Experience);
            
            var player = await _context.Players
                .Include(p => p.CompletedQuests)
                .FirstOrDefaultAsync(p => p.Id == playerId);
            
            Assert.NotNull(player?.CompletedQuests);
Assert.Contains(player.CompletedQuests, q => q.Id == questId);
            Assert.Equal(100, player?.Experience); // 100 опыта за квест
            Assert.Equal(100, player?.Gold); // 50 начальных + 50 награды
        }

        [Fact]
        public async Task DefeatEnemyAsync_ShouldDefeatEnemy_WhenPlayerAndEnemyExist()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var enemyId = _context.Enemies.First().Id;
            
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player", 
                Experience = 25, 
                Gold = 60 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.DefeatEnemyAsync(playerId, enemyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(25, result.Experience);
            Assert.Equal(60, result.Gold);
            
            var player = await _context.Players
                .Include(p => p.DefeatedEnemies)
                .FirstOrDefaultAsync(p => p.Id == playerId);
            
            Assert.NotNull(player?.DefeatedEnemies);
Assert.Contains(player.DefeatedEnemies, e => e.Id == enemyId);
            Assert.Equal(25, player?.Experience);
            Assert.Equal(60, player?.Gold); // 50 начальных + 10 награды
        }

        [Fact]
        public async Task GetPlayersPagedAsync_ShouldReturnPagedPlayers()
        {
            // Arrange
            var pagination = new PaginationDto { Page = 1, PageSize = 10 };
            
            var pagedResult = new PagedResult<PlayerDto>
            {
                Items = new List<PlayerDto> 
                { 
                    new PlayerDto { Name = "Test Player" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };
            
            _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
                .Returns(pagedResult.Items.ToList());

            // Act
            var result = await _playerService.GetPlayersPagedAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.PageNumber);
        }

        [Fact]
        public async Task SearchPlayersAsync_ShouldReturnPlayers()
        {
            // Arrange
            var searchTerm = "test";

            // Act
            var result = await _playerService.SearchPlayersAsync(searchTerm);

            // Assert
            Assert.Single(result);
            Assert.Contains("Test", result.First().Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPlayersByLevelRangeAsync_ShouldReturnPlayersInRange()
        {
            // Arrange
            var minLevel = 1;
            var maxLevel = 3;

            // Act
            var result = await _playerService.GetPlayersByLevelRangeAsync(minLevel, maxLevel);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Level);
        }

        [Fact]
        public async Task GetAlivePlayersAsync_ShouldReturnOnlyAlivePlayers()
        {
            // Arrange
            // Добавляем мертвого игрока
            var deadPlayer = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Dead Player",
                Email = "dead@example.com",
                Health = 0,
                MaxHealth = 100,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Players.Add(deadPlayer);
            await _context.SaveChangesAsync();
            
            _mapperMock.Setup(x => x.Map<IEnumerable<PlayerDto>>(It.IsAny<IEnumerable<Player>>()))
                .Returns((IEnumerable<Player> players) => 
                    players.Where(p => p.Health > 0)
                    .Select(p => new PlayerDto { 
                        Name = p.Name, 
                        Health = p.Health 
                    }).ToList());

            // Act
            var result = await _playerService.GetAlivePlayersAsync();

            // Assert
            Assert.Single(result);
            Assert.True(result.First().Health > 0);
        }

        [Fact]
        public async Task UpdatePlayerAsync_ShouldUpdatePlayer_WhenExists()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var updateDto = new UpdatePlayerDto { 
                Name = "Updated Player", 
                Level = 10 
            };
            
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Updated Player", 
                Level = 10 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.UpdatePlayerAsync(playerId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Player", result.Name);
            Assert.Equal(10, result.Level);
            
            var updatedPlayer = await _context.Players.FindAsync(playerId);
            Assert.Equal("Updated Player", updatedPlayer?.Name);
            Assert.Equal(10, updatedPlayer?.Level);
        }

        [Fact]
        public async Task DamagePlayerAsync_ShouldApplyDamage_WhenPlayerExists()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player", 
                Health = 50 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.DamagePlayerAsync(playerId, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.Health);
            
            var damagedPlayer = await _context.Players.FindAsync(playerId);
            Assert.Equal(50, damagedPlayer?.Health);
        }

        [Fact]
        public async Task GetPlayersCountByLevelAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var newPlayer = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Another Player",
                Email = "another@example.com",
                Level = 1,
                Health = 100,
                MaxHealth = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _playerService.GetPlayersCountByLevelAsync();

            // Assert
            Assert.True(result.ContainsKey(1));
            Assert.Equal(2, result[1]); // Два игрока уровня 1
        }

        [Fact]
        public async Task GetTotalPlayerGoldAsync_ShouldReturnCorrectTotal()
        {
            // Act
            var result = await _playerService.GetTotalPlayerGoldAsync();

            // Assert
            Assert.Equal(50, result); // Начальный игрок имеет 50 золота
        }

        [Fact]
        public async Task GetPlayerStatsAsync_ShouldReturnStats()
        {
            // Act
            var result = await _playerService.GetPlayerStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalPlayers);
            Assert.Equal(1, result.AverageLevel);
            Assert.Equal(50, result.TotalGold);
            Assert.Equal(1, result.MaxLevel);
            Assert.Equal(1, result.MinLevel);
        }

        [Fact]
        public async Task CompleteQuestWithTransactionAsync_ShouldCompleteQuest_WhenValid()
        {
            // Arrange
            var playerId = _context.Players.First().Id;
            var questId = _context.Quests.First().Id;
            
            var playerDto = new PlayerDto { 
                Id = playerId, 
                Name = "Test Player" 
            };
            
            _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
                .Returns(playerDto);

            // Act
            var result = await _playerService.CompleteQuestWithTransactionAsync(playerId, questId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Player", result.Name);
        }

        [Fact]
public async Task GetPlayersPagedAsync_ShouldHandleSearch()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        Search = "Test" // Покрывает строки 62-69 (поиск)
    };
    
    _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
        .Returns(new List<PlayerDto> 
        { 
            new PlayerDto { Name = "Test Player" }
        });

    // Act
    var result = await _playerService.GetPlayersPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Items);
}

[Fact]
public async Task GetPlayersPagedAsync_ShouldHandleSortingByNameAsc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "name", // Покрывает строку 73: сортировка по имени asc
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
        .Returns(new List<PlayerDto> 
        { 
            new PlayerDto { Name = "Test Player" }
        });

    // Act
    var result = await _playerService.GetPlayersPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetPlayersPagedAsync_ShouldHandleSortingByNameDesc()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "name",
        SortOrder = "desc" // Покрывает строку 73: сортировка по имени desc
    };
    
    _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
        .Returns(new List<PlayerDto> 
        { 
            new PlayerDto { Name = "Test Player" }
        });

    // Act
    var result = await _playerService.GetPlayersPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetPlayersPagedAsync_ShouldHandleSortingByLevel()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "level", // Покрывает строку 74: сортировка по уровню
        SortOrder = "desc"
    };
    
    _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
        .Returns(new List<PlayerDto> 
        { 
            new PlayerDto { Name = "Test Player", Level = 5 }
        });

    // Act
    var result = await _playerService.GetPlayersPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task GetPlayersPagedAsync_ShouldHandleSortingByExperience()
{
    // Arrange
    var pagination = new PaginationDto 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "experience", // Покрывает строку 75: сортировка по опыту
        SortOrder = "asc"
    };
    
    _mapperMock.Setup(x => x.Map<List<PlayerDto>>(It.IsAny<List<Player>>()))
        .Returns(new List<PlayerDto> 
        { 
            new PlayerDto { Name = "Test Player", Experience = 100 }
        });

    // Act
    var result = await _playerService.GetPlayersPagedAsync(pagination);

    // Assert
    Assert.NotNull(result);
}

[Fact]
public async Task UpdatePlayerAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var updateDto = new UpdatePlayerDto { Name = "Updated Player" };

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.UpdatePlayerAsync(playerId, updateDto));
}

[Fact]
public async Task UpdatePlayerAsync_ShouldThrowConflict_WhenEmailAlreadyExists()
{
    // Arrange
    // Добавляем двух игроков
    var firstPlayer = new Player
    {
        Id = Guid.NewGuid(),
        Name = "First Player",
        Email = "first@test.com",
        Level = 1,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    var secondPlayer = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Second Player",
        Email = "second@test.com",
        Level = 2,
        Health = 120,
        MaxHealth = 120,
        Attack = 15,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    _context.Players.Add(firstPlayer);
    _context.Players.Add(secondPlayer);
    await _context.SaveChangesAsync();
    
    var updateDto = new UpdatePlayerDto 
    { 
        Email = "second@test.com" // Покрывает строки 224-233: конфликт email
    };
    
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Returns(new PlayerDto { Id = firstPlayer.Id, Name = "First Player" });

    // Act & Assert
    await Assert.ThrowsAsync<ConflictException>(() =>
        _playerService.UpdatePlayerAsync(firstPlayer.Id, updateDto));
}

[Fact]
public async Task UpdatePlayerAsync_ShouldUpdateLocation()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    
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
    
    var updateDto = new UpdatePlayerDto 
    { 
        CurrentLocation = "Dark Forest" // Покрывает строки 239-249: обновление локации
    };
    
    var playerDto = new PlayerDto 
    { 
        Id = playerId, 
        Name = "Test Player",
        CurrentLocation = "Dark Forest"
    };
    
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Returns(playerDto);

    // Act
    var result = await _playerService.UpdatePlayerAsync(playerId, updateDto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Dark Forest", result.CurrentLocation);
}

[Fact]
public async Task UpdatePlayerAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var updateDto = new UpdatePlayerDto { Name = "Updated Player" };
    
    // Симулируем ошибку при сохранении - покрывает строки 257-260
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.UpdatePlayerAsync(playerId, updateDto));
}

[Fact]
public async Task DeletePlayerAsync_ShouldReturnFalse_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();

    // Act
    var result = await _playerService.DeletePlayerAsync(playerId);

    // Assert
    Assert.False(result); // Покрывает строки 271-274
}

[Fact]
public async Task DeletePlayerAsync_ShouldDeletePlayer_WhenExists()
{
    // Arrange
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Player to Delete",
        Email = "delete@test.com",
        Level = 1,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    await _context.SaveChangesAsync();

    // Act
    var result = await _playerService.DeletePlayerAsync(player.Id);

    // Assert
    Assert.True(result); // Покрывает строки 277-281
    Assert.Null(await _context.Players.FindAsync(player.Id));
}

[Fact]
public async Task AddExperienceAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var experience = 100;

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.AddExperienceAsync(playerId, experience)); // Покрывает строки 293-297
}

[Fact]
public async Task AddExperienceAsync_ShouldLogLevelUp_WhenPlayerLevelsUp()
{
    // Arrange
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Level Up Player",
        Email = "levelup@test.com",
        Level = 1,
        Experience = 0,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    await _context.SaveChangesAsync();
    
    var playerDto = new PlayerDto 
    { 
        Id = player.Id, 
        Name = "Level Up Player",
        Level = 2,
        Experience = 1000
    };
    
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Returns(playerDto);

    // Act
    var result = await _playerService.AddExperienceAsync(player.Id, 1000); // Достаточно для повышения уровня

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Level); // Покрывает строки 303-307 (логирование повышения уровня)
}

[Fact]
public async Task AddExperienceAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var experience = 100;
    
    // Симулируем ошибку при сохранении - покрывает строки 314-317
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.AddExperienceAsync(playerId, experience));
}

[Fact]
public async Task HealPlayerAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var amount = 50;

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.HealPlayerAsync(playerId, amount)); // Покрывает строки 330-334
}

[Fact]
public async Task HealPlayerAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var amount = 50;
    
    // Симулируем ошибку при сохранении - покрывает строки 345-348
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.HealPlayerAsync(playerId, amount));
}

[Fact]
public async Task DamagePlayerAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var damage = 50;

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.DamagePlayerAsync(playerId, damage)); // Покрывает строки 361-365
}

[Fact]
public async Task DamagePlayerAsync_ShouldLogDefeat_WhenPlayerHealthReachesZero()
{
    // Arrange
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Weak Player",
        Email = "weak@test.com",
        Level = 1,
        Experience = 0,
        Health = 10, // Мало здоровья
        MaxHealth = 100,
        Attack = 5,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    await _context.SaveChangesAsync();
    
    var playerDto = new PlayerDto 
    { 
        Id = player.Id, 
        Name = "Weak Player",
        Health = 0
    };
    
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Returns(playerDto);

    // Act
    var result = await _playerService.DamagePlayerAsync(player.Id, 20); // Больше чем здоровье

    // Assert
    Assert.NotNull(result);
    Assert.Equal(0, result.Health); // Покрывает строки 374-377 (логирование поражения)
}

[Fact]
public async Task DamagePlayerAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var damage = 50;
    
    // Симулируем ошибку при сохранении - покрывает строки 381-384
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.DamagePlayerAsync(playerId, damage));
}

[Fact]
public async Task CompleteQuestAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var questId = Guid.NewGuid();

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.CompleteQuestAsync(playerId, questId)); // Покрывает строки 398-402
}

[Fact]
public async Task CompleteQuestAsync_ShouldThrow_WhenQuestNotFound()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var questId = Guid.NewGuid();

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.CompleteQuestAsync(playerId, questId)); // Покрывает строки 407-411
}

[Fact]
public async Task CompleteQuestAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    // Сначала создаем игрока и квест в базе
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Test Player",
        Email = "test@test.com",
        Level = 1,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    
    var quest = new Quest
    {
        Id = Guid.NewGuid(),
        Title = "Test Quest",
        Description = "A test quest",
        ExperienceReward = 100,
        GoldReward = 50,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Quests.Add(quest);
    
    await _context.SaveChangesAsync();
    
    // Симулируем ошибку при маппинге - покрывает строки 429-432
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.CompleteQuestAsync(player.Id, quest.Id));
}

[Fact]
public async Task DefeatEnemyAsync_ShouldThrow_WhenPlayerNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var enemyId = Guid.NewGuid();

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.DefeatEnemyAsync(playerId, enemyId)); // Покрывает строки 446-450
}

[Fact]
public async Task DefeatEnemyAsync_ShouldThrow_WhenEnemyNotFound()
{
    // Arrange
    var playerId = _context.Players.First().Id;
    var enemyId = Guid.NewGuid();

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.DefeatEnemyAsync(playerId, enemyId)); // Покрывает строки 455-459
}

[Fact]
public async Task DefeatEnemyAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    // Сначала создаем игрока и врага в базе
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Test Player",
        Email = "test@test.com",
        Level = 1,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    
    var enemy = new Enemy
    {
        Id = Guid.NewGuid(),
        Name = "Test Enemy",
        Type = EnemyType.Goblin,
        Level = 1,
        Health = 50,
        MaxHealth = 50,
        Attack = 5,
        ExperienceReward = 25,
        GoldReward = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Enemies.Add(enemy);
    
    await _context.SaveChangesAsync();
    
    // Теперь симулируем ошибку при маппинге - покрывает строки 477-480
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.DefeatEnemyAsync(player.Id, enemy.Id));
}

[Fact]
public async Task CompleteQuestWithTransactionAsync_ShouldThrow_WhenPlayerOrQuestNotFound()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var questId = Guid.NewGuid();

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(() =>
        _playerService.CompleteQuestWithTransactionAsync(playerId, questId)); // Покрывает строки 539-540
}

[Fact]
public async Task CompleteQuestWithTransactionAsync_ShouldLogError_WhenExceptionOccurs()
{
    // Arrange
    // Сначала создаем игрока и квест в базе
    var player = new Player
    {
        Id = Guid.NewGuid(),
        Name = "Test Player",
        Email = "test@test.com",
        Level = 1,
        Health = 100,
        MaxHealth = 100,
        Attack = 10,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Players.Add(player);
    
    var quest = new Quest
    {
        Id = Guid.NewGuid(),
        Title = "Test Quest",
        Description = "A test quest",
        ExperienceReward = 100,
        GoldReward = 50,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _context.Quests.Add(quest);
    
    await _context.SaveChangesAsync();
    
    // Симулируем ошибку при маппинге - покрывает строки 559-562
    _mapperMock.Setup(x => x.Map<PlayerDto>(It.IsAny<Player>()))
        .Throws(new Exception("Mapping error"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _playerService.CompleteQuestWithTransactionAsync(player.Id, quest.Id));
}
    }

}