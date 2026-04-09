namespace RpgGame.Core.Models
{
    public class Player : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Attack { get; set; } = 10;
        public int Gold { get; set; } = 50;
        public Guid? LocationId { get; set; }
        public virtual GameLocation? CurrentGameLocation { get; set; }

        public int ExperienceToNextLevel => 100;
        public bool IsAlive => Health > 0;

        // Навигационные свойства
        public virtual ICollection<Quest> CompletedQuests { get; set; } = new List<Quest>();
        public virtual ICollection<Enemy> DefeatedEnemies { get; set; } = new List<Enemy>();
        public virtual ICollection<Item> InventoryItems { get; set; } = new List<Item>();

        public void AddExperience(int amount)
        {
            Experience += amount;

            int levelUps = 0;
            int maxLevelUps = 10;

            while (Experience >= ExperienceToNextLevel && levelUps < maxLevelUps)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                MaxHealth += 20;
                Attack += 5;
                Health = MaxHealth;
                levelUps++;
            }

            UpdateTimestamps();
        }

        public void TakeDamage(int damage)
        {
            Health = Math.Max(Health - damage, 0);
            UpdateTimestamps();
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            if (amount >= 30)
                Health = MaxHealth;
            else
                Health = Math.Min(Health + amount, MaxHealth);

            UpdateTimestamps();
        }

        public void CompleteQuest(Quest quest)
        {
            if (!CompletedQuests.Any(q => q.Id == quest.Id))
            {
                CompletedQuests.Add(quest);
                Experience += quest.ExperienceReward;
                Gold += quest.GoldReward;
                UpdateTimestamps();
            }
        }

        public void DefeatEnemy(Enemy enemy)
        {
            if (!DefeatedEnemies.Any(e => e.Id == enemy.Id))
            {
                DefeatedEnemies.Add(enemy);
                Experience += enemy.ExperienceReward;
                Gold += enemy.GoldReward;
                UpdateTimestamps();
            }
        }

        public bool HasCompletedQuest(Guid questId) => CompletedQuests.Any(q => q.Id == questId);
        public bool HasDefeatedEnemy(Guid enemyId) => DefeatedEnemies.Any(e => e.Id == enemyId);
    }
}