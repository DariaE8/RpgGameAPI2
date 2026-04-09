namespace RpgGame.Core.Models
{
    public class Enemy : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public EnemyType Type { get; set; }
        public int Level { get; set; } = 1;
        public int Health { get; set; } = 50;
        public int MaxHealth { get; set; } = 50;
        public int Attack { get; set; } = 10;
        public int ExperienceReward { get; set; } = 25;
        public int GoldReward { get; set; } = 10;

 
        public Guid? LocationId { get; set; }
        public virtual GameLocation? GameLocation { get; set; }

        public bool IsAlive => Health > 0;

        public virtual ICollection<Player> DefeatedByPlayers { get; set; } = new List<Player>();
        public virtual ICollection<Quest> RequiredForQuests { get; set; } = new List<Quest>();

        // Бизнес-логика
        public void TakeDamage(int damage)
        {
            Health = Math.Max(Health - damage, 0);
            UpdateTimestamps();
        }

        public bool CanBeLooted() => !IsAlive;

        public int CalculateReward()
        {
            return ExperienceReward + (GoldReward * Level);
        }
    }

    public enum EnemyType
    {
        Goblin,
        Orc,
        Dragon,
        Skeleton,
        Spider
    }
}