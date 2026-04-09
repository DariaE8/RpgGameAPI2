using Xunit;
using RpgGame.Core.Models;

namespace RpgGame.Tests.UnitTests
{
    public class StorageSettingsTests
    {
        [Fact]
        public void StorageSettings_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var settings = new StorageSettings();

            // Assert
            Assert.Equal("Data", settings.DataPath);
            Assert.Equal("players.json", settings.PlayersFile);
            Assert.Equal("enemies.json", settings.EnemiesFile);
            Assert.Equal("quests.json", settings.QuestsFile);
            Assert.Equal("locations.json", settings.LocationsFile);
        }

        [Fact]
        public void StorageSettings_ShouldAllowCustomValues()
        {
            // Arrange & Act
            var settings = new StorageSettings
            {
                DataPath = "CustomData",
                PlayersFile = "custom_players.json",
                EnemiesFile = "custom_enemies.json",
                QuestsFile = "custom_quests.json",
                LocationsFile = "custom_locations.json"
            };

            // Assert
            Assert.Equal("CustomData", settings.DataPath);
            Assert.Equal("custom_players.json", settings.PlayersFile);
            Assert.Equal("custom_enemies.json", settings.EnemiesFile);
            Assert.Equal("custom_quests.json", settings.QuestsFile);
            Assert.Equal("custom_locations.json", settings.LocationsFile);
        }
    }
}