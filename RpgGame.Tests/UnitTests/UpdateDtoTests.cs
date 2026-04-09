using Xunit;
using System.ComponentModel.DataAnnotations;
using RpgGame.Core.DTOs;

namespace RpgGame.Tests.UnitTests
{
    public class UpdateDtoTests
    {
        [Fact]
        public void UpdatePlayerDto_AllPropertiesShouldBeNullable()
        {
            // Arrange & Act
            var dto = new UpdatePlayerDto();

            // Assert
            Assert.Null(dto.Name);
            Assert.Null(dto.Email);
            Assert.Null(dto.Level);
            Assert.Null(dto.Health);
            Assert.Null(dto.MaxHealth);
            Assert.Null(dto.Attack);
            Assert.Null(dto.Gold);
            Assert.Null(dto.CurrentLocation);
        }

        [Fact]
        public void UpdateEnemyDto_AllPropertiesShouldBeNullable()
        {
            // Arrange & Act
            var dto = new UpdateEnemyDto();

            // Assert
            Assert.Null(dto.Name);
            Assert.Null(dto.Type);
            Assert.Null(dto.Level);
            Assert.Null(dto.Health);
            Assert.Null(dto.MaxHealth);
            Assert.Null(dto.Attack);
            Assert.Null(dto.ExperienceReward);
            Assert.Null(dto.GoldReward);
            Assert.Null(dto.Location);
        }

        [Fact]
        public void UpdateQuestDto_AllPropertiesShouldBeNullable()
        {
            // Arrange & Act
            var dto = new UpdateQuestDto();

            // Assert
            Assert.Null(dto.Title);
            Assert.Null(dto.Description);
            Assert.Null(dto.Objective);
            Assert.Null(dto.TargetCount);
            Assert.Null(dto.ExperienceReward);
            Assert.Null(dto.GoldReward);
            Assert.Null(dto.RequiredItemIds);
            Assert.Null(dto.RequiredEnemyTypes);
            Assert.Null(dto.RequiredLocation);
        }

        [Fact]
        public void UpdateGameLocationDto_AllPropertiesShouldBeNullable()
        {
            // Arrange & Act
            var dto = new UpdateGameLocationDto();

            // Assert
            Assert.Null(dto.Name);
            Assert.Null(dto.Description);
            Assert.Null(dto.Type);
            Assert.Null(dto.RequiredLevel);
            Assert.Null(dto.AvailableEnemies);
            Assert.Null(dto.AvailableQuests);
            Assert.Null(dto.IsSafeZone);
        }

        [Fact]
        public void UpdatePlayerDto_ShouldAllowPartialUpdates()
        {
            // Arrange
            var dto = new UpdatePlayerDto
            {
                Name = "UpdatedName",
                Level = 5
                // Other properties remain null
            };

            // Act & Assert
            Assert.Equal("UpdatedName", dto.Name);
            Assert.Equal(5, dto.Level);
            Assert.Null(dto.Email);
            Assert.Null(dto.Health);
        }

        [Fact]
        public void UpdateEnemyDto_ShouldAllowPartialUpdates()
        {
            // Arrange
            var dto = new UpdateEnemyDto
            {
                Health = 75,
                Attack = 15
                // Other properties remain null
            };

            // Act & Assert
            Assert.Equal(75, dto.Health);
            Assert.Equal(15, dto.Attack);
            Assert.Null(dto.Name);
            Assert.Null(dto.Level);
        }

        [Fact]
        public void UpdateQuestDto_ShouldAllowCollectionUpdates()
        {
            // Arrange
            var itemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var enemyTypes = new List<string> { "Goblin", "Orc" };

            var dto = new UpdateQuestDto
            {
                RequiredItemIds = itemIds,
                RequiredEnemyTypes = enemyTypes
            };

            // Act & Assert
            Assert.Equal(itemIds, dto.RequiredItemIds);
            Assert.Equal(enemyTypes, dto.RequiredEnemyTypes);
        }

        [Fact]
        public void UpdateGameLocationDto_ShouldAllowCollectionUpdates()
        {
            // Arrange
            var enemies = new List<string> { "Goblin", "Orc" };
            var quests = new List<Guid> { Guid.NewGuid() };

            var dto = new UpdateGameLocationDto
            {
                AvailableEnemies = enemies,
                AvailableQuests = quests
            };

            // Act & Assert
            Assert.Equal(enemies, dto.AvailableEnemies);
            Assert.Equal(quests, dto.AvailableQuests);
        }
    }
}