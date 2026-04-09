namespace RpgGame.Core.DTOs
{
    public class EnemyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsAlive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}