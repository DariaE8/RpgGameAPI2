using System.ComponentModel.DataAnnotations;

namespace RpgGame.Core.DTOs
{
    public class UpdatePlayerDto
    {
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Range(1, 100, ErrorMessage = "Level must be between 1 and 100")]
        public int? Level { get; set; }

        [Range(1, 1000, ErrorMessage = "Health must be between 1 and 1000")]
        public int? Health { get; set; }

        [Range(1, 1000, ErrorMessage = "MaxHealth must be between 1 and 1000")]
        public int? MaxHealth { get; set; }

        [Range(1, 100, ErrorMessage = "Attack must be between 1 and 100")]
        public int? Attack { get; set; }

        [Range(0, 9999, ErrorMessage = "Gold must be between 0 and 9999")]
        public int? Gold { get; set; }

        [StringLength(50, ErrorMessage = "CurrentLocation cannot exceed 50 characters")]
        public string? CurrentLocation { get; set; }
    }
}