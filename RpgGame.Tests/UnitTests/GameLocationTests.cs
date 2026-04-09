using Xunit;
using RpgGame.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace RpgGame.API.Tests.Core.Models;

public class GameLocationTests
{
    [Fact]
    public void CanPlayerAccess_WhenPlayerLevelGreaterOrEqual_ShouldReturnTrue()
    {
        // Arrange
        var location = new GameLocation { RequiredLevel = 10 };
        
        // Act & Assert
        Assert.True(location.CanPlayerAccess(10));    // Равный уровень
        Assert.True(location.CanPlayerAccess(15));    // Выше уровень
        Assert.False(location.CanPlayerAccess(5));    // Ниже уровень
    }

    [Fact]
    public void CanPlayerAccess_WithDefaultRequiredLevel_ShouldAllowLevel1AndAbove()
    {
        // Arrange
        var location = new GameLocation(); // RequiredLevel = 1 по умолчанию
        
        // Act & Assert
        Assert.True(location.CanPlayerAccess(1));
        Assert.False(location.CanPlayerAccess(0));
    }

    [Fact]
    public void HasEnemies_WhenEnemiesListIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var location = new GameLocation();
        
        // Act & Assert
        Assert.False(location.HasEnemies());
    }

    [Fact]
    public void GetLocationInfo_ShouldReturnCorrectFormatForSafeZone()
    {
        // Arrange
        var location = new GameLocation
        {
            Name = "Peaceful Village",
            RequiredLevel = 5,
            IsSafeZone = true
        };
        
        // Act
        var result = location.GetLocationInfo();
        
        // Assert
        Assert.Equal("Peaceful Village (Level 5+) - Safe Zone", result);
    }

    [Fact]
    public void GetLocationInfo_ShouldReturnCorrectFormatForDangerZone()
    {
        // Arrange
        var location = new GameLocation
        {
            Name = "Dark Forest",
            RequiredLevel = 15,
            IsSafeZone = false
        };
        
        // Act
        var result = location.GetLocationInfo();
        
        // Assert
        Assert.Equal("Dark Forest (Level 15+) - Danger Zone", result);
    }

    [Fact]
    public void GetLocationInfo_WithDefaultValues_ShouldReturnDefaultInfo()
    {
        // Arrange
        var location = new GameLocation(); // Name = "", RequiredLevel = 1, IsSafeZone = false
        
        // Act
        var result = location.GetLocationInfo();
        
        // Assert
        Assert.Equal(" (Level 1+) - Danger Zone", result);
    }

    [Fact]
    public void GetLocationInfo_WithSpecialCharactersInName_ShouldHandleCorrectly()
    {
        // Arrange
        var location = new GameLocation
        {
            Name = "Dragon's Lair",
            RequiredLevel = 30,
            IsSafeZone = false
        };
        
        // Act
        var result = location.GetLocationInfo();
        
        // Assert
        Assert.Equal("Dragon's Lair (Level 30+) - Danger Zone", result);
    }

    [Fact]
    public void EnemiesCollection_ShouldBeInitializedByDefault()
    {
        // Arrange & Act
        var location = new GameLocation();
        
        // Assert
        Assert.NotNull(location.Enemies);
        Assert.Empty(location.Enemies);
    }

    [Fact]
    public void QuestsCollection_ShouldBeInitializedByDefault()
    {
        // Arrange & Act
        var location = new GameLocation();
        
        // Assert
        Assert.NotNull(location.Quests);
        Assert.Empty(location.Quests);
    }

    [Fact]
    public void PlayersCollection_ShouldBeInitializedByDefault()
    {
        // Arrange & Act
        var location = new GameLocation();
        
        // Assert
        Assert.NotNull(location.Players);
        Assert.Empty(location.Players);
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var location = new GameLocation();
        
        // Assert
        Assert.Equal(string.Empty, location.Name);
        Assert.Equal(string.Empty, location.Description);
        Assert.Equal(1, location.RequiredLevel);
        Assert.False(location.IsSafeZone);
    }

    [Fact]
    public void LocationType_ShouldHaveCorrectEnumValues()
    {
        // Act & Assert
        var values = System.Enum.GetValues(typeof(LocationType));
        Assert.Equal(6, values.Length);
        
        var names = System.Enum.GetNames(typeof(LocationType));
        Assert.Contains("Forest", names);
        Assert.Contains("Cave", names);
        Assert.Contains("Castle", names);
        Assert.Contains("Village", names);
        Assert.Contains("Dungeon", names);
        Assert.Contains("Mountain", names);
    }

    [Fact]
    public void Collections_ShouldBeMutable()
    {
        // Arrange
        var location = new GameLocation();
        var enemy = new Enemy();
        var quest = new Quest();
        var player = new Player();
        
        // Act
        location.Enemies.Add(enemy);
        location.Quests.Add(quest);
        location.Players.Add(player);
        
        // Assert
        Assert.Single(location.Enemies);
        Assert.Contains(enemy, location.Enemies);
        
        Assert.Single(location.Quests);
        Assert.Contains(quest, location.Quests);
        
        Assert.Single(location.Players);
        Assert.Contains(player, location.Players);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var location = new GameLocation();
        
        // Assert
        Assert.IsAssignableFrom<BaseEntity>(location);
    }
}