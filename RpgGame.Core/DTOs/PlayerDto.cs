namespace RpgGame.Core.DTOs
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Gold { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public bool IsAlive { get; set; }

        public int CompletedQuestsCount { get; set; }
        public int InventoryItemsCount { get; set; }
        public int DefeatedEnemiesCount { get; set; }

        public List<Guid> CompletedQuestIds { get; set; } = new();
        public List<Guid> InventoryItemIds { get; set; } = new();
        public List<Guid> DefeatedEnemyIds { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}