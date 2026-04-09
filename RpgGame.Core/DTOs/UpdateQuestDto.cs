using System.ComponentModel.DataAnnotations;
using RpgGame.Core.Models;

namespace RpgGame.Core.DTOs
{
    public class UpdateQuestDto
    {
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Objective cannot exceed 200 characters")]
        public string? Objective { get; set; }

        [Range(1, 100, ErrorMessage = "TargetCount must be between 1 and 100")]
        public int? TargetCount { get; set; }

        [Range(0, 10000, ErrorMessage = "ExperienceReward must be between 0 and 10000")]
        public int? ExperienceReward { get; set; }

        [Range(0, 5000, ErrorMessage = "GoldReward must be between 0 and 5000")]
        public int? GoldReward { get; set; }

        public List<Guid>? RequiredItemIds { get; set; }

        public List<string>? RequiredEnemyTypes { get; set; }

        [StringLength(50, ErrorMessage = "RequiredLocation cannot exceed 50 characters")]
        public string? RequiredLocation { get; set; }
    }
}