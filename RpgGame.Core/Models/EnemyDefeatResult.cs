public class EnemyDefeatResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ExperienceGained { get; set; }
    public int? GoldReward { get; set; }
    public List<string>? Loot { get; set; }
}