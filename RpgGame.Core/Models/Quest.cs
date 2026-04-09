namespace RpgGame.Core.Models
{
    public class Quest : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public int TargetCount { get; set; } = 3;
        public int CurrentCount { get; set; } = 0;
        public int ExperienceReward { get; set; } = 100;
        public int GoldReward { get; set; } = 50;
        public QuestStatus Status { get; set; } = QuestStatus.Available;

        public Guid? LocationId { get; set; }
        public virtual GameLocation? GameLocation { get; set; }
        public virtual ICollection<Player> PlayersCompleted { get; set; } = new List<Player>();
        public virtual ICollection<Enemy> RequiredEnemies { get; set; } = new List<Enemy>();
        public virtual ICollection<Item> RequiredItems { get; set; } = new List<Item>();

        public double Progress => TargetCount > 0 ? Math.Min((double)CurrentCount / TargetCount * 100, 100) : 0;
        public bool IsCompleted => CurrentCount >= TargetCount;

        public void UpdateProgress(int amount = 1)
        {
            CurrentCount = Math.Min(CurrentCount + amount, TargetCount);
            if (IsCompleted)
                Status = QuestStatus.Completed;

            UpdateTimestamps();
        }
    }

    public enum QuestStatus
    {
        Available,
        InProgress,
        Completed,
        Failed
    }
}