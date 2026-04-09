using Xunit;
using RpgGame.Core.DTOs;

namespace RpgGame.Tests.UnitTests
{
    public class LocationStatsDtoTests
    {
        [Fact]
        public void LocationStatsDto_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var totalLocations = 10;
            var safeZonesCount = 3;
            var dangerousZonesCount = 7;
            var averageRequiredLevel = 5.5;

            // Act
            var dto = new LocationStatsDto
            {
                TotalLocations = totalLocations,
                SafeZonesCount = safeZonesCount,
                DangerousZonesCount = dangerousZonesCount,
                AverageRequiredLevel = averageRequiredLevel
            };

            // Assert
            Assert.Equal(totalLocations, dto.TotalLocations);
            Assert.Equal(safeZonesCount, dto.SafeZonesCount);
            Assert.Equal(dangerousZonesCount, dto.DangerousZonesCount);
            Assert.Equal(averageRequiredLevel, dto.AverageRequiredLevel);
        }

        [Fact]
        public void LocationStatsDto_DefaultValues_ShouldBeZero()
        {
            // Act
            var dto = new LocationStatsDto();

            // Assert
            Assert.Equal(0, dto.TotalLocations);
            Assert.Equal(0, dto.SafeZonesCount);
            Assert.Equal(0, dto.DangerousZonesCount);
            Assert.Equal(0.0, dto.AverageRequiredLevel);
        }

        [Fact]
        public void LocationStatsDto_ShouldAllowNegativeValues()
        {
            // Act
            var dto = new LocationStatsDto
            {
                TotalLocations = -5,
                SafeZonesCount = -2,
                DangerousZonesCount = -3,
                AverageRequiredLevel = -1.5
            };

            // Assert
            Assert.Equal(-5, dto.TotalLocations);
            Assert.Equal(-2, dto.SafeZonesCount);
            Assert.Equal(-3, dto.DangerousZonesCount);
            Assert.Equal(-1.5, dto.AverageRequiredLevel);
        }
    }
}