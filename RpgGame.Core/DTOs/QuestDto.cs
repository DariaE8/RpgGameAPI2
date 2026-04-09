namespace RpgGame.Core.DTOs
{
    public class QuestDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public int TargetCount { get; set; }
        public int CurrentCount { get; set; }
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public bool IsCompleted { get; set; }
        
        // Новые поля
        public List<Guid> RequiredItemIds { get; set; } = new();
        public List<string> RequiredEnemyTypes { get; set; } = new();
        public string RequiredLocation { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
    }
}