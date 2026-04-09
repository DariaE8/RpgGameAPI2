public class QuestCompletionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ExperienceGained { get; set; }
    public int? GoldReward { get; set; }
    public List<string>? ItemRewards { get; set; }
}
