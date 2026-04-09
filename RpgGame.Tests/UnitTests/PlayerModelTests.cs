using Xunit;
using RpgGame.Core.Models;

namespace RpgGame.Tests.UnitTests
{
    public class PlayerModelTests
    {
        [Fact]
        public void Player_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var player = new Player();

            // Assert
            Assert.Equal(string.Empty, player.Name);
            Assert.Equal(string.Empty, player.Email);
            Assert.Equal(1, player.Level);
            Assert.Equal(0, player.Experience);
            Assert.Equal(100, player.Health);
            Assert.Equal(100, player.MaxHealth);
            Assert.Equal(10, player.Attack);
            Assert.Equal(50, player.Gold);
            Assert.Null(player.LocationId);
            Assert.Null(player.CurrentGameLocation);
            Assert.True(player.IsAlive);
            Assert.Equal(100, player.ExperienceToNextLevel);
            Assert.Empty(player.CompletedQuests);
            Assert.Empty(player.DefeatedEnemies);
            Assert.Empty(player.InventoryItems);
        }

        [Theory]
        [InlineData(50, 1, 50)]    // No level up
        [InlineData(100, 2, 0)]    // Exact level up
        [InlineData(150, 2, 50)]   // Level up with leftover experience
        [InlineData(250, 3, 50)]   // Multiple level ups
        public void AddExperience_ShouldWorkCorrectly(int experienceToAdd, int expectedLevel, int expectedExperience)
        {
            // Arrange
            var player = new Player();

            // Act
            player.AddExperience(experienceToAdd);

            // Assert
            Assert.Equal(expectedLevel, player.Level);
            Assert.Equal(expectedExperience, player.Experience);
        }

        [Fact]
        public void LevelUp_ShouldIncreaseStatsAndHeal()
        {
            // Arrange
            var player = new Player { Health = 50 };

            // Act
            player.AddExperience(100);

            // Assert
            Assert.Equal(2, player.Level);
            Assert.Equal(120, player.MaxHealth); // 100 + 20
            Assert.Equal(15, player.Attack);     // 10 + 5
            Assert.Equal(120, player.Health);    // Full heal
        }

        [Theory]
        [InlineData(30, 70)]
        [InlineData(100, 0)]
        [InlineData(150, 0)]
        public void TakeDamage_ShouldReduceHealthCorrectly(int damage, int expectedHealth)
        {
            // Arrange
            var player = new Player { Health = 100 };

            // Act
            player.TakeDamage(damage);

            // Assert
            Assert.Equal(expectedHealth, player.Health);
        }

        [Theory]
        [InlineData(30, 100)]  // Большое лечение = полное восстановление
        [InlineData(50, 100)]  // Большое лечение = полное восстановление
        [InlineData(10, 60)]   // Малое лечение
        public void Heal_ShouldIncreaseHealthCorrectly(int healAmount, int expectedHealth)
        {
            // Arrange
            var player = new Player { Health = 50 };

            // Act
            player.Heal(healAmount);

            // Assert
            Assert.Equal(expectedHealth, player.Health);
        }

        [Fact]
        public void Heal_ShouldDoNothing_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var player = new Player { Health = 50 };

            // Act
            player.Heal(0);
            player.Heal(-10);

            // Assert
            Assert.Equal(50, player.Health);
        }

        [Fact]
        public void CompleteQuest_ShouldAddRewardsAndMarkCompleted()
        {
            // Arrange
            var player = new Player();
            var quest = new Quest 
            { 
                Id = Guid.NewGuid(),
                ExperienceReward = 100,
                GoldReward = 50
            };

            // Act
            player.CompleteQuest(quest);

            // Assert
            Assert.Contains(quest, player.CompletedQuests);
            Assert.Equal(100, player.Experience);
            Assert.Equal(100, player.Gold); // 50 начальных + 50 награды
        }

        [Fact]
        public void CompleteQuest_ShouldNotAddDuplicateQuest()
        {
            // Arrange
            var player = new Player();
            var quest = new Quest { Id = Guid.NewGuid(), ExperienceReward = 100, GoldReward = 50 };

            // Act
            player.CompleteQuest(quest);
            player.CompleteQuest(quest); // Второй раз

            // Assert
            Assert.Single(player.CompletedQuests);
            Assert.Equal(100, player.Experience); // Не должно добавиться второй раз
            Assert.Equal(100, player.Gold);      // Не должно добавиться второй раз
        }

        [Fact]
        public void DefeatEnemy_ShouldAddRewardsAndMarkDefeated()
        {
            // Arrange
            var player = new Player();
            var enemy = new Enemy 
            { 
                Id = Guid.NewGuid(),
                ExperienceReward = 25,
                GoldReward = 10
            };

            // Act
            player.DefeatEnemy(enemy);

            // Assert
            Assert.Contains(enemy, player.DefeatedEnemies);
            Assert.Equal(25, player.Experience);
            Assert.Equal(60, player.Gold); // 50 начальных + 10 награды
        }

        [Fact]
        public void DefeatEnemy_ShouldNotAddDuplicateEnemy()
        {
            // Arrange
            var player = new Player();
            var enemy = new Enemy { Id = Guid.NewGuid(), ExperienceReward = 25, GoldReward = 10 };

            // Act
            player.DefeatEnemy(enemy);
            player.DefeatEnemy(enemy); // Второй раз

            // Assert
            Assert.Single(player.DefeatedEnemies);
            Assert.Equal(25, player.Experience); // Не должно добавиться второй раз
            Assert.Equal(60, player.Gold);      // Не должно добавиться второй раз
        }

        [Fact]
        public void HasCompletedQuest_ShouldReturnCorrectValue()
        {
            // Arrange
            var player = new Player();
            var quest = new Quest { Id = Guid.NewGuid() };
            player.CompletedQuests.Add(quest);

            // Act & Assert
            Assert.True(player.HasCompletedQuest(quest.Id));
            Assert.False(player.HasCompletedQuest(Guid.NewGuid()));
        }

        [Fact]
        public void HasDefeatedEnemy_ShouldReturnCorrectValue()
        {
            // Arrange
            var player = new Player();
            var enemy = new Enemy { Id = Guid.NewGuid() };
            player.DefeatedEnemies.Add(enemy);

            // Act & Assert
            Assert.True(player.HasDefeatedEnemy(enemy.Id));
            Assert.False(player.HasDefeatedEnemy(Guid.NewGuid()));
        }

        [Fact]
        public void IsAlive_ShouldReturnTrue_WhenHealthGreaterThanZero()
        {
            // Arrange
            var player1 = new Player { Health = 100 };
            var player2 = new Player { Health = 1 };
            var player3 = new Player { Health = 0 };

            // Act & Assert
            Assert.True(player1.IsAlive);
            Assert.True(player2.IsAlive);
            Assert.False(player3.IsAlive);
        }

        [Fact]
        public void AddExperience_ShouldCapLevelUps_WhenTooMuchExperience()
        {
            // Arrange
            var player = new Player();

            // Act - Добавляем огромное количество опыта (должно вызвать максимум 10 уровней)
            player.AddExperience(5000);

            // Assert
            Assert.Equal(11, player.Level); // Начальный уровень 1 + максимум 10 повышений
            Assert.True(player.Experience >= 0); // Оставшийся опыт
        }
    }
}