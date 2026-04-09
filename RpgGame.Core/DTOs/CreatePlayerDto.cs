using System.ComponentModel.DataAnnotations;

namespace RpgGame.Core.DTOs
{
    public class CreatePlayerDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Level is required")]
        [Range(1, 100, ErrorMessage = "Level must be between 1 and 100")]
        public int Level { get; set; } = 1;

        [Required(ErrorMessage = "Health is required")]
        [Range(1, 1000, ErrorMessage = "Health must be between 1 and 1000")]
        public int Health { get; set; } = 100;

        [Required(ErrorMessage = "MaxHealth is required")]
        [Range(1, 1000, ErrorMessage = "MaxHealth must be between 1 and 1000")]
        public int MaxHealth { get; set; } = 100;

        [Required(ErrorMessage = "Attack is required")]
        [Range(1, 100, ErrorMessage = "Attack must be between 1 and 100")]
        public int Attack { get; set; } = 10;

        [Required(ErrorMessage = "Gold is required")]
        [Range(0, 9999, ErrorMessage = "Gold must be between 0 and 9999")]
        public int Gold { get; set; } = 50;

        [Required(ErrorMessage = "CurrentLocation is required")]
        [StringLength(50, ErrorMessage = "CurrentLocation cannot exceed 50 characters")]
        public string CurrentLocation { get; set; } = "forest";
    }
}