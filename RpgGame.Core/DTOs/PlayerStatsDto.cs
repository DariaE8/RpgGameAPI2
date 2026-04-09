namespace RpgGame.Core.DTOs
{
    public class PlayerStatsDto
    {
        public int TotalPlayers { get; set; }
        public double AverageLevel { get; set; }
        public int TotalGold { get; set; }
        public int MaxLevel { get; set; }
        public int MinLevel { get; set; }
    }
}