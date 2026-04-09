using System.ComponentModel.DataAnnotations;
using RpgGame.Core.Models;

namespace RpgGame.Core.DTOs
{
    public class CreateEnemyDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public EnemyType Type { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Range(1, 100, ErrorMessage = "Level must be between 1 and 100")]
        public int Level { get; set; } = 1;

        [Required(ErrorMessage = "Health is required")]
        [Range(1, 1000, ErrorMessage = "Health must be between 1 and 1000")]
        public int Health { get; set; } = 50;

        [Required(ErrorMessage = "MaxHealth is required")]
        [Range(1, 1000, ErrorMessage = "MaxHealth must be between 1 and 1000")]
        public int MaxHealth { get; set; } = 50;

        [Required(ErrorMessage = "Attack is required")]
        [Range(1, 100, ErrorMessage = "Attack must be between 1 and 100")]
        public int Attack { get; set; } = 10;

        [Required(ErrorMessage = "ExperienceReward is required")]
        [Range(0, 1000, ErrorMessage = "ExperienceReward must be between 0 and 1000")]
        public int ExperienceReward { get; set; } = 25;

        [Required(ErrorMessage = "GoldReward is required")]
        [Range(0, 500, ErrorMessage = "GoldReward must be between 0 and 500")]
        public int GoldReward { get; set; } = 10;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(50, ErrorMessage = "Location cannot exceed 50 characters")]
        public string Location { get; set; } = "forest";
    }
}