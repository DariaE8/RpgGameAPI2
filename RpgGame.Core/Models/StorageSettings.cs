namespace RpgGame.Core.Models
{
    public class StorageSettings
    {
        public string DataPath { get; set; } = "Data";
        public string PlayersFile { get; set; } = "players.json";
        public string EnemiesFile { get; set; } = "enemies.json";
        public string QuestsFile { get; set; } = "quests.json";
        public string LocationsFile { get; set; } = "locations.json";
    }
}