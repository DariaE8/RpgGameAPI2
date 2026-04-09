using FluentAssertions;

namespace RpgGame.API.Tests.Core.DTOs;

public class EnemyDefeatResultTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var result = new EnemyDefeatResult();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().BeEmpty();
        result.ExperienceGained.Should().Be(0);
        result.GoldReward.Should().BeNull();
        result.Loot.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var loot = new List<string> { "Sword", "Shield", "Potion" };
        var result = new EnemyDefeatResult();

        // Act
        result.Success = true;
        result.Message = "Enemy defeated!";
        result.ExperienceGained = 100;
        result.GoldReward = 50;
        result.Loot = loot;

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Enemy defeated!");
        result.ExperienceGained.Should().Be(100);
        result.GoldReward.Should().Be(50);
        result.Loot.Should().BeSameAs(loot);
        result.Loot.Should().HaveCount(3);
        result.Loot.Should().Contain("Sword");
        result.Loot.Should().Contain("Shield");
        result.Loot.Should().Contain("Potion");
    }

    [Fact]
    public void GoldReward_ShouldBeNullable()
    {
        // Arrange
        var result = new EnemyDefeatResult();

        // Act - установить значение
        result.GoldReward = 100;
        result.GoldReward.Should().Be(100);

        // Act - установить null
        result.GoldReward = null;
        result.GoldReward.Should().BeNull();

        // Act - установить 0 (допустимое значение)
        result.GoldReward = 0;
        result.GoldReward.Should().Be(0);
    }

    [Fact]
    public void Loot_ShouldBeNullableAndModifiable()
    {
        // Arrange
        var result = new EnemyDefeatResult();
        var loot = new List<string> { "Item1" };

        // Act & Assert - можно установить список
        result.Loot = loot;
        result.Loot.Should().BeSameAs(loot);

        // Act & Assert - можно установить null
        result.Loot = null;
        result.Loot.Should().BeNull();

        // Act & Assert - можно установить пустой список
        result.Loot = new List<string>();
        result.Loot.Should().BeEmpty();
    }

    [Fact]
    public void Message_ShouldBeEmptyStringByDefault()
    {
        // Arrange & Act
        var result = new EnemyDefeatResult();

        // Assert
        result.Message.Should().BeEmpty();
        result.Message.Should().Be("");
    }

    [Fact]
    public void ExperienceGained_CanBeNegative_ForEdgeCases()
    {
        // Arrange
        var result = new EnemyDefeatResult();

        // Act
        result.ExperienceGained = -10; // Например, если игрок потерял опыт

        // Assert
        result.ExperienceGained.Should().Be(-10);
    }

    [Fact]
    public void ShouldSupportObjectInitializerSyntax()
    {
        // Arrange & Act
        var result = new EnemyDefeatResult
        {
            Success = true,
            Message = "Victory!",
            ExperienceGained = 150,
            GoldReward = 75,
            Loot = new List<string> { "Magic Ring", "Scroll" }
        };

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Victory!");
        result.ExperienceGained.Should().Be(150);
        result.GoldReward.Should().Be(75);
        result.Loot.Should().HaveCount(2);
        result.Loot.Should().Contain("Magic Ring");
    }

    [Fact]
    public void Equality_ShouldWorkByReference()
    {
        // Arrange
        var result1 = new EnemyDefeatResult { Success = true };
        var result2 = new EnemyDefeatResult { Success = true };
        var result3 = result1;

        // Act & Assert
        result1.Should().NotBeSameAs(result2);
        result1.Should().BeSameAs(result3);
        result1.Equals(result2).Should().BeFalse(); // Разные объекты
    }

    [Fact]
    public void ToString_ShouldNotThrow()
    {
        // Arrange
        var result = new EnemyDefeatResult
        {
            Success = true,
            Message = "Test",
            ExperienceGained = 100
        };

        // Act & Assert
        var act = () => result.ToString();
        act.Should().NotThrow();
        
        // Можно проверить, что возвращает не null/empty
        result.ToString().Should().NotBeNullOrEmpty();
    }
}