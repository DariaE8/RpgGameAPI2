namespace RpgGame.Core.DTOs
{
    public class GameLocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int RequiredLevel { get; set; }
        public List<string> AvailableEnemies { get; set; } = new(); // Исправлено
        public List<Guid> AvailableQuests { get; set; } = new();   // Исправлено
        public bool IsSafeZone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}