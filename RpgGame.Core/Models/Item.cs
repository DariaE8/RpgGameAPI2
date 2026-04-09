namespace RpgGame.Core.Models
{
    public class Item : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public int AttackModifier { get; set; }
        public int HealthModifier { get; set; }
        public int Value { get; set; }
        public bool IsEquippable => Type != ItemType.Consumable;

        
    public virtual ICollection<Player> OwnedByPlayers { get; set; } = new List<Player>();
    public virtual ICollection<Quest> RequiredForQuests { get; set; } = new List<Quest>();

    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest
    }

}