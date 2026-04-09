using Xunit;
using RpgGame.Core.Models;
using System;

namespace RpgGame.Tests.UnitTests
{
    public class EnemyModelTests
    {
        [Fact]
        public void Enemy_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var enemy = new Enemy();

            // Assert
            Assert.Equal(string.Empty, enemy.Name);
            Assert.Equal(1, enemy.Level);
            Assert.Equal(50, enemy.Health);
            Assert.Equal(50, enemy.MaxHealth);
            Assert.Equal(10, enemy.Attack);
            Assert.Equal(25, enemy.ExperienceReward);
            Assert.Equal(10, enemy.GoldReward);
            // 🔥 УБРАЛИ: Assert.Equal("forest", enemy.Location); - теперь это связь
            Assert.True(enemy.IsAlive);
            
            // Проверяем навигационные свойства инициализированы
            Assert.NotNull(enemy.DefeatedByPlayers);
            Assert.NotNull(enemy.RequiredForQuests);
            Assert.Empty(enemy.DefeatedByPlayers);
            Assert.Empty(enemy.RequiredForQuests);
        }

        [Theory]
        [InlineData(10, 40)]
        [InlineData(50, 0)]
        [InlineData(100, 0)]
        public void TakeDamage_ShouldReduceHealthCorrectly(int damage, int expectedHealth)
        {
            // Arrange
            var enemy = new Enemy { Health = 50 };

            // Act
            enemy.TakeDamage(damage);

            // Assert
            Assert.Equal(expectedHealth, enemy.Health);
        }

        [Theory]
        [InlineData(50, true)]   // Health = 0 → CanBeLooted = true
        [InlineData(40, false)]  // Health > 0 → CanBeLooted = false
        [InlineData(100, true)]  // Health < 0 (clamped to 0) → CanBeLooted = true
        public void CanBeLooted_ShouldReturnCorrectValue(int damage, bool expected)
        {
            // Arrange
            var enemy = new Enemy { Health = 50 };
            enemy.TakeDamage(damage);

            // Act
            var result = enemy.CanBeLooted();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, 35)]  // 25 + (10 * 1) = 35
        [InlineData(5, 75)]  // 25 + (10 * 5) = 75  
        [InlineData(10, 125)] // 25 + (10 * 10) = 125
        public void CalculateReward_ShouldCalculateCorrectly(int level, int expectedReward)
        {
            // Arrange
            var enemy = new Enemy 
            { 
                Level = level,
                ExperienceReward = 25,
                GoldReward = 10
            };

            // Act
            var reward = enemy.CalculateReward();

            // Assert
            Assert.Equal(expectedReward, reward);
        }

        [Fact]
        public void IsAlive_ShouldReturnFalseWhenHealthIsZero()
        {
            // Arrange
            var enemy = new Enemy { Health = 0 };

            // Act & Assert
            Assert.False(enemy.IsAlive);
        }

        [Fact]
        public void IsAlive_ShouldReturnTrueWhenHealthIsPositive()
        {
            // Arrange
            var enemy = new Enemy { Health = 10 };

            // Act & Assert
            Assert.True(enemy.IsAlive);
        }

        [Fact]
        public void TakeDamage_ShouldUpdateTimestamps()
        {
            // Arrange
            var enemy = new Enemy();
            var originalUpdatedAt = enemy.UpdatedAt;
            
            // Ждем немного чтобы время изменилось
            System.Threading.Thread.Sleep(1);

            // Act
            enemy.TakeDamage(10);

            // Assert
            Assert.True(enemy.UpdatedAt > originalUpdatedAt);
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка связи с GameLocation
        [Fact]
        public void Enemy_GameLocation_ShouldBeNullable()
        {
            // Arrange & Act
            var enemy = new Enemy();

            // Assert
            Assert.Null(enemy.GameLocation); // Связь может быть null
            Assert.Null(enemy.LocationId);   // Id связи может быть null
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка навигационных свойств
        [Fact]
        public void Enemy_NavigationProperties_ShouldBeInitialized()
        {
            // Arrange & Act
            var enemy = new Enemy();

            // Assert
            Assert.NotNull(enemy.DefeatedByPlayers);
            Assert.NotNull(enemy.RequiredForQuests);
            Assert.Empty(enemy.DefeatedByPlayers);
            Assert.Empty(enemy.RequiredForQuests);
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка методов бизнес-логики
        [Fact]
        public void TakeDamage_ShouldClampHealthToZero()
        {
            // Arrange
            var enemy = new Enemy { Health = 10 };

            // Act
            enemy.TakeDamage(50); // Больше чем здоровье

            // Assert
            Assert.Equal(0, enemy.Health);
            Assert.False(enemy.IsAlive);
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка вычисляемых свойств
        [Fact]
        public void Enemy_ShouldHaveCorrectDefaultValuesAfterDamage()
        {
            // Arrange
            var enemy = new Enemy();
            var originalHealth = enemy.Health;

            // Act
            enemy.TakeDamage(30);

            // Assert
            Assert.Equal(20, enemy.Health);
            Assert.True(enemy.IsAlive);
            Assert.False(enemy.CanBeLooted());
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка перечисления EnemyType
        [Fact]
        public void Enemy_Type_ShouldHaveValidEnumValue()
        {
            // Arrange & Act
            var enemy = new Enemy { Type = EnemyType.Goblin };

            // Assert
            Assert.Equal(EnemyType.Goblin, enemy.Type);
            Assert.IsType<EnemyType>(enemy.Type);
        }

        // 🔥 НОВЫЙ ТЕСТ: Проверка формулы награды с кастомными значениями
        [Fact]
        public void CalculateReward_WithCustomValues_ShouldCalculateCorrectly()
        {
            // Arrange
            var enemy = new Enemy 
            { 
                Level = 3,
                ExperienceReward = 50,
                GoldReward = 20
            };

            // Act
            var reward = enemy.CalculateReward();

            // Assert
            // Формула: ExperienceReward + (GoldReward * Level)
            // 50 + (20 * 3) = 50 + 60 = 110
            Assert.Equal(110, reward);
        }
    }
}