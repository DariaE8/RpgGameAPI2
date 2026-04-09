using Xunit;
using RpgGame.Core.Models;

namespace RpgGame.Tests.UnitTests
{
    public class QuestModelTests
    {
        [Fact]
        public void Quest_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var quest = new Quest();

            // Assert
            Assert.Equal(string.Empty, quest.Title);
            Assert.Equal(string.Empty, quest.Description);
            Assert.Equal(string.Empty, quest.Objective);
            Assert.Equal(3, quest.TargetCount);
            Assert.Equal(0, quest.CurrentCount);
            Assert.Equal(100, quest.ExperienceReward);
            Assert.Equal(50, quest.GoldReward);
            Assert.Equal(QuestStatus.Available, quest.Status);
            Assert.Equal(0, quest.Progress);
            Assert.False(quest.IsCompleted);
        }

[Theory]
[InlineData(1, 33.33)]
[InlineData(2, 66.67)]
[InlineData(3, 100)]
[InlineData(5, 100)] // Should not exceed 100%
public void Progress_ShouldCalculateCorrectly(int currentCount, double expectedProgress)
{
    // Arrange
    var quest = new Quest { TargetCount = 3, CurrentCount = currentCount };

    // Act & Assert
    Assert.Equal(expectedProgress, quest.Progress, 2);
}

        [Theory]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(5, true)]
        public void IsCompleted_ShouldReturnCorrectValue(int currentCount, bool expected)
        {
            // Arrange
            var quest = new Quest { TargetCount = 3, CurrentCount = currentCount };

            // Act & Assert
            Assert.Equal(expected, quest.IsCompleted);
        }

        [Fact]
        public void UpdateProgress_ShouldIncreaseCurrentCount()
        {
            // Arrange
            var quest = new Quest { TargetCount = 3 };

            // Act
            quest.UpdateProgress(2);

            // Assert
            Assert.Equal(2, quest.CurrentCount);
            Assert.Equal(66.67, quest.Progress, 2);
        }

        [Fact]
        public void UpdateProgress_ShouldNotExceedTargetCount()
        {
            // Arrange
            var quest = new Quest { TargetCount = 3, CurrentCount = 2 };

            // Act
            quest.UpdateProgress(5);

            // Assert
            Assert.Equal(3, quest.CurrentCount);
            Assert.True(quest.IsCompleted);
        }

        [Fact]
        public void UpdateProgress_ShouldSetStatusToCompletedWhenTargetReached()
        {
            // Arrange
            var quest = new Quest { TargetCount = 3, CurrentCount = 2 };

            // Act
            quest.UpdateProgress(1);

            // Assert
            Assert.Equal(QuestStatus.Completed, quest.Status);
            Assert.True(quest.IsCompleted);
        }

        [Fact]
        public void UpdateProgress_ShouldUpdateTimestamps()
        {
            // Arrange
            var quest = new Quest();
            var originalUpdatedAt = quest.UpdatedAt;

            // Act
            quest.UpdateProgress(1);

            // Assert
            Assert.True(quest.UpdatedAt > originalUpdatedAt);
        }
    }
}