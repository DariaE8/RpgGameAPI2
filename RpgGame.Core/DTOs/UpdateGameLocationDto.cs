using System.ComponentModel.DataAnnotations;
using RpgGame.Core.Models;

namespace RpgGame.Core.DTOs
{
    public class UpdateGameLocationDto
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public LocationType? Type { get; set; }

        [Range(1, 100, ErrorMessage = "RequiredLevel must be between 1 and 100")]
        public int? RequiredLevel { get; set; }

        public bool? IsSafeZone { get; set; }

        public List<string>? AvailableEnemies { get; set; }

        public List<Guid>? AvailableQuests { get; set; }
    }
}