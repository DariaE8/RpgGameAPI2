using System.ComponentModel.DataAnnotations;
using RpgGame.Core.Models;

namespace RpgGame.Core.DTOs
{
    public class CreateGameLocationDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public LocationType Type { get; set; }

        [Required(ErrorMessage = "RequiredLevel is required")]
        [Range(1, 100, ErrorMessage = "RequiredLevel must be between 1 and 100")]
        public int RequiredLevel { get; set; } = 1;

        [Required(ErrorMessage = "IsSafeZone is required")]
        public bool IsSafeZone { get; set; } = false;

        public List<string> AvailableEnemies { get; set; } = new();

        public List<Guid> AvailableQuests { get; set; } = new();
    }
}