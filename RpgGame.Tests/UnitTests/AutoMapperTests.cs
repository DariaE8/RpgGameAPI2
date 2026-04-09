using Xunit;
using AutoMapper;
using RpgGame.Core.DTOs;
using RpgGame.Core.Models;
using RpgGame.Services.Mappings;
using System;
using System.Collections.Generic;

namespace RpgGame.Tests.UnitTests
{
    public class AutoMapperTests
    {
        private readonly IMapper _mapper;

        public AutoMapperTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<GameMappingProfile>();
            });

            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void Configuration_ShouldBeValid()
        {
            // Arrange & Act & Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void PlayerToPlayerDto_ShouldMapCorrectly()
        {
            // Arrange
            var location = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Forest",
                Type = LocationType.Forest
            };

            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = "TestPlayer",
                Email = "test@example.com",
                Level = 5,
                Health = 80,
                MaxHealth = 100,
                Attack = 15,
                Gold = 200,
                CurrentGameLocation = location, // 🔥 ИСПРАВЛЕНО: CurrentGameLocation вместо CurrentLocation
                CreatedAt = DateTime.UtcNow,
                CompletedQuests = new List<Quest>(),
                DefeatedEnemies = new List<Enemy>(),
                InventoryItems = new List<Item>()
            };

            // Act
            var result = _mapper.Map<PlayerDto>(player);

            // Assert
            Assert.Equal(player.Id, result.Id);
            Assert.Equal(player.Name, result.Name);
            Assert.Equal(player.Email, result.Email);
            Assert.Equal(player.Level, result.Level);
            Assert.Equal(player.Health, result.Health);
            Assert.Equal(player.MaxHealth, result.MaxHealth);
            Assert.Equal(player.Attack, result.Attack);
            Assert.Equal(player.Gold, result.Gold);
            Assert.Equal("Forest", result.CurrentLocation); // 🔥 МАППИТСЯ ИЗ CurrentGameLocation.Name
            Assert.Equal(player.CreatedAt, result.CreatedAt);
            Assert.Equal(player.IsAlive, result.IsAlive);
            Assert.Equal(0, result.CompletedQuestsCount); // 🔥 Считается из CompletedQuests.Count
            Assert.Equal(0, result.DefeatedEnemiesCount); // 🔥 Считается из DefeatedEnemies.Count
        }

        [Fact]
        public void CreatePlayerDtoToPlayer_ShouldMapCorrectly()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "NewPlayer",
                Email = "new@example.com"
            };

            // Act
            var result = _mapper.Map<Player>(createDto);

            // Assert
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Email, result.Email);
            Assert.Equal(1, result.Level); // Default value
            Assert.Equal(100, result.Health); // Default value
            // 🔥 УБРАЛИ CurrentLocation - теперь это связь
        }

        [Fact]
        public void EnemyToEnemyDto_ShouldMapTypeAsString()
        {
            // Arrange
            var location = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Forest",
                Type = LocationType.Forest
            };

            var enemy = new Enemy
            {
                Id = Guid.NewGuid(),
                Name = "Goblin",
                Type = EnemyType.Goblin,
                Level = 3,
                Health = 30,
                MaxHealth = 30,
                Attack = 8,
                ExperienceReward = 25,
                GoldReward = 10,
                GameLocation = location, // 🔥 ИСПРАВЛЕНО: GameLocation вместо Location
                CreatedAt = DateTime.UtcNow,
                DefeatedByPlayers = new List<Player>(),
                RequiredForQuests = new List<Quest>()
            };

            // Act
            var result = _mapper.Map<EnemyDto>(enemy);

            // Assert
            Assert.Equal(enemy.Id, result.Id);
            Assert.Equal(enemy.Name, result.Name);
            Assert.Equal("Goblin", result.Type); // Should be string
            Assert.Equal(enemy.Level, result.Level);
            Assert.Equal(enemy.Health, result.Health);
            Assert.Equal(enemy.MaxHealth, result.MaxHealth);
            Assert.Equal(enemy.Attack, result.Attack);
            Assert.Equal(enemy.ExperienceReward, result.ExperienceReward);
            Assert.Equal(enemy.GoldReward, result.GoldReward);
            Assert.Equal("Forest", result.Location); // 🔥 МАППИТСЯ ИЗ GameLocation.Name
            Assert.Equal(enemy.CreatedAt, result.CreatedAt);
            Assert.Equal(enemy.IsAlive, result.IsAlive);
        }

        [Fact]
        public void CreateEnemyDtoToEnemy_ShouldMapCorrectly()
        {
            // Arrange
            var createDto = new CreateEnemyDto
            {
                Name = "Dragon",
                Type = EnemyType.Dragon,
                Level = 10,
                Health = 200,
                MaxHealth = 200,
                Attack = 25,
                ExperienceReward = 100,
                GoldReward = 50
                // 🔥 УБРАЛИ Location - теперь это связь
            };

            // Act
            var result = _mapper.Map<Enemy>(createDto);

            // Assert
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Type, result.Type);
            Assert.Equal(createDto.Level, result.Level);
            Assert.Equal(createDto.Health, result.Health);
            Assert.Equal(createDto.MaxHealth, result.MaxHealth);
            Assert.Equal(createDto.Attack, result.Attack);
            Assert.Equal(createDto.ExperienceReward, result.ExperienceReward);
            Assert.Equal(createDto.GoldReward, result.GoldReward);
            // 🔥 УБРАЛИ Location - теперь это связь
        }

        [Fact]
        public void QuestToQuestDto_ShouldMapStatusAsString()
        {
            // Arrange
            var location = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Forest",
                Type = LocationType.Forest
            };

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Test Quest",
                Description = "Test Description",
                Objective = "Test Objective",
                TargetCount = 5,
                CurrentCount = 3,
                ExperienceReward = 100,
                GoldReward = 50,
                Status = QuestStatus.InProgress,
                GameLocation = location, // 🔥 ИСПРАВЛЕНО: GameLocation вместо RequiredLocation
                CreatedAt = DateTime.UtcNow,
                PlayersCompleted = new List<Player>(),
                RequiredEnemies = new List<Enemy>(),
                RequiredItems = new List<Item>()
            };

            // Act
            var result = _mapper.Map<QuestDto>(quest);

            // Assert
            Assert.Equal(quest.Id, result.Id);
            Assert.Equal(quest.Title, result.Title);
            Assert.Equal(quest.Description, result.Description);
            Assert.Equal(quest.Objective, result.Objective);
            Assert.Equal(quest.TargetCount, result.TargetCount);
            Assert.Equal(quest.CurrentCount, result.CurrentCount);
            Assert.Equal(quest.ExperienceReward, result.ExperienceReward);
            Assert.Equal(quest.GoldReward, result.GoldReward);
            Assert.Equal("InProgress", result.Status); // Should be string
            Assert.Equal(quest.Progress, result.Progress);
            Assert.Equal(quest.IsCompleted, result.IsCompleted);
            Assert.Equal("Forest", result.RequiredLocation); // 🔥 МАППИТСЯ ИЗ GameLocation.Name
            Assert.Equal(quest.CreatedAt, result.CreatedAt);
        }

        [Fact]
        public void CreateQuestDtoToQuest_ShouldMapCorrectly()
        {
            // Arrange
            var createDto = new CreateQuestDto
            {
                Title = "New Quest",
                Description = "New Description",
                Objective = "New Objective",
                TargetCount = 10,
                ExperienceReward = 200,
                GoldReward = 100
            };

            // Act
            var result = _mapper.Map<Quest>(createDto);

            // Assert
            Assert.Equal(createDto.Title, result.Title);
            Assert.Equal(createDto.Description, result.Description);
            Assert.Equal(createDto.Objective, result.Objective);
            Assert.Equal(createDto.TargetCount, result.TargetCount);
            Assert.Equal(createDto.ExperienceReward, result.ExperienceReward);
            Assert.Equal(createDto.GoldReward, result.GoldReward);
            Assert.Equal(QuestStatus.Available, result.Status); // Default value
            Assert.Equal(0, result.CurrentCount); // Default value
            // 🔥 УБРАЛИ RequiredLocation - теперь это связь
        }

        [Fact]
        public void GameLocationToGameLocationDto_ShouldMapTypeAsString()
        {
            // Arrange
            var location = new GameLocation
            {
                Id = Guid.NewGuid(),
                Name = "Dark Forest",
                Description = "A dangerous forest",
                Type = LocationType.Forest,
                RequiredLevel = 5,
                IsSafeZone = false,
                CreatedAt = DateTime.UtcNow,
                Enemies = new List<Enemy>(), // 🔥 ИСПРАВЛЕНО: Enemies вместо AvailableEnemies
                Quests = new List<Quest>(),   // 🔥 ИСПРАВЛЕНО: Quests вместо AvailableQuests
                Players = new List<Player>()
            };

            // Act
            var result = _mapper.Map<GameLocationDto>(location);

            // Assert
            Assert.Equal(location.Id, result.Id);
            Assert.Equal(location.Name, result.Name);
            Assert.Equal(location.Description, result.Description);
            Assert.Equal("Forest", result.Type); // Should be string
            Assert.Equal(location.RequiredLevel, result.RequiredLevel);
            Assert.Equal(location.IsSafeZone, result.IsSafeZone);
            Assert.Equal(location.CreatedAt, result.CreatedAt);
            // 🔥 УБРАЛИ AvailableEnemies и AvailableQuests - теперь это связи
        }
    }
}