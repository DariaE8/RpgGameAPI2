namespace RpgGame.Core.DTOs
{
    public class LocationStatsDto
    {
        public int TotalLocations { get; set; }
        public int SafeZonesCount { get; set; }
        public int DangerousZonesCount { get; set; }
        public double AverageRequiredLevel { get; set; }
    }
}