using FluentAssertions;

namespace RpgGame.API.Tests.Core.DTOs;

public class QuestCompletionResultTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var result = new QuestCompletionResult();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().BeEmpty();
        result.ExperienceGained.Should().Be(0);
        result.GoldReward.Should().BeNull();
        result.ItemRewards.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var itemRewards = new List<string> { "Magic Sword", "Healing Potion", "Gold Key" };
        var result = new QuestCompletionResult();

        // Act
        result.Success = true;
        result.Message = "Quest completed successfully!";
        result.ExperienceGained = 500;
        result.GoldReward = 200;
        result.ItemRewards = itemRewards;

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Quest completed successfully!");
        result.ExperienceGained.Should().Be(500);
        result.GoldReward.Should().Be(200);
        result.ItemRewards.Should().BeSameAs(itemRewards);
        result.ItemRewards.Should().HaveCount(3);
        result.ItemRewards.Should().Contain("Magic Sword");
        result.ItemRewards.Should().Contain("Healing Potion");
        result.ItemRewards.Should().Contain("Gold Key");
    }

    [Fact]
    public void GoldReward_ShouldSupportNullAndZeroValues()
    {
        // Arrange
        var result = new QuestCompletionResult();

        // Act & Assert - null значение
        result.GoldReward = null;
        result.GoldReward.Should().BeNull();

        // Act & Assert - положительное значение
        result.GoldReward = 1000;
        result.GoldReward.Should().Be(1000);

        // Act & Assert - нулевое значение
        result.GoldReward = 0;
        result.GoldReward.Should().Be(0);

        // Act & Assert - отрицательное значение (если такое возможно)
        result.GoldReward = -50;
        result.GoldReward.Should().Be(-50);
    }

    [Fact]
    public void ItemRewards_ShouldBeFullyMutable()
    {
        // Arrange
        var result = new QuestCompletionResult();
        var rewards = new List<string> { "Item1", "Item2" };

        // Act - установить и изменить список
        result.ItemRewards = rewards;
        
        // Добавить элемент в существующий список
        result.ItemRewards!.Add("Item3");

        // Assert
        result.ItemRewards.Should().HaveCount(3);
        result.ItemRewards.Should().Contain("Item3");

        // Act - заменить список полностью
        result.ItemRewards = new List<string> { "NewItem" };
        result.ItemRewards.Should().HaveCount(1);
        result.ItemRewards.Should().Contain("NewItem");

        // Act - установить null
        result.ItemRewards = null;
        result.ItemRewards.Should().BeNull();
    }

    [Fact]
    public void ShouldHandleFailedQuestScenario()
    {
        // Arrange & Act
        var result = new QuestCompletionResult
        {
            Success = false,
            Message = "Quest failed: Time ran out",
            ExperienceGained = 0, // Нет опыта за провал
            GoldReward = null,    // Нет золота
            ItemRewards = null    // Нет наград
        };

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Quest failed: Time ran out");
        result.ExperienceGained.Should().Be(0);
        result.GoldReward.Should().BeNull();
        result.ItemRewards.Should().BeNull();
    }

    [Fact]
    public void ShouldHandleSuccessfulQuestWithPartialRewards()
    {
        // Arrange & Act
        var result = new QuestCompletionResult
        {
            Success = true,
            Message = "Quest completed with bonus!",
            ExperienceGained = 750,
            GoldReward = 150, // Только золото, без предметов
            ItemRewards = null
        };

        // Assert
        result.Success.Should().BeTrue();
        result.ExperienceGained.Should().Be(750);
        result.GoldReward.Should().Be(150);
        result.ItemRewards.Should().BeNull();
    }

    [Fact]
    public void ShouldHandleSuccessfulQuestWithItemsOnly()
    {
        // Arrange & Act
        var result = new QuestCompletionResult
        {
            Success = true,
            Message = "Quest completed! Received special items.",
            ExperienceGained = 300,
            GoldReward = null, // Нет золота
            ItemRewards = new List<string> { "Ancient Relic", "Secret Map" }
        };

        // Assert
        result.Success.Should().BeTrue();
        result.GoldReward.Should().BeNull();
        result.ItemRewards.Should().HaveCount(2);
    }

    [Fact]
    public void Message_CanBeVeryLong()
    {
        // Arrange
        var longMessage = new string('A', 1000); // Очень длинное сообщение
        var result = new QuestCompletionResult();

        // Act
        result.Message = longMessage;

        // Assert
        result.Message.Should().Be(longMessage);
        result.Message.Length.Should().Be(1000);
    }

    [Fact]
    public void ExperienceGained_CanBeLargeValue()
    {
        // Arrange
        var result = new QuestCompletionResult();

        // Act
        result.ExperienceGained = int.MaxValue; // Максимальное значение int

        // Assert
        result.ExperienceGained.Should().Be(int.MaxValue);

        // Act
        result.ExperienceGained = int.MinValue; // Минимальное значение

        // Assert
        result.ExperienceGained.Should().Be(int.MinValue);
    }

    [Fact]
    public void ShouldWorkWithObjectInitializer()
    {
        // Arrange & Act
        var result = new QuestCompletionResult
        {
            Success = true,
            Message = "Epic quest complete!",
            ExperienceGained = 10000,
            GoldReward = 5000,
            ItemRewards = new List<string>
            {
                "Dragon Scale Armor",
                "Phoenix Feather",
                "Elven Bow"
            }
        };

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Epic quest complete!");
        result.ExperienceGained.Should().Be(10000);
        result.GoldReward.Should().Be(5000);
        result.ItemRewards.Should().HaveCount(3);
    }

    [Fact]
    public void ToString_ShouldReturnValidString()
    {
        // Arrange
        var result = new QuestCompletionResult
        {
            Success = true,
            Message = "Test Quest"
        };

        // Act & Assert
        result.ToString().Should().NotBeNullOrEmpty();
        result.ToString().Should().Contain("QuestCompletionResult"); // Обычно содержит имя класса
    }
}