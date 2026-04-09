namespace RpgGame.Core.Models
{
    public class GameLocation : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LocationType Type { get; set; }
        public int RequiredLevel { get; set; } = 1;
        public bool IsSafeZone { get; set; } = false;

        public virtual ICollection<Enemy> Enemies { get; set; } = new List<Enemy>();
        public virtual ICollection<Quest> Quests { get; set; } = new List<Quest>();
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        
        // Бизнес-логика
        public bool CanPlayerAccess(int playerLevel) => playerLevel >= RequiredLevel;
        
        public bool HasEnemies() => Enemies.Any(e => e.IsAlive);
        
        public string GetLocationInfo()
        {
            return $"{Name} (Level {RequiredLevel}+) - {(IsSafeZone ? "Safe Zone" : "Danger Zone")}";
        }
    }

    public enum LocationType
    {
        Forest,
        Cave,
        Castle,
        Village,
        Dungeon,
        Mountain
    }
}