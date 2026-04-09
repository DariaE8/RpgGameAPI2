using System.ComponentModel.DataAnnotations;
using RpgGame.Core.Models;

namespace RpgGame.Core.DTOs
{
    public class CreateQuestDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Objective is required")]
        [StringLength(200, ErrorMessage = "Objective cannot exceed 200 characters")]
        public string Objective { get; set; } = string.Empty;

        [Required(ErrorMessage = "TargetCount is required")]
        [Range(1, 100, ErrorMessage = "TargetCount must be between 1 and 100")]
        public int TargetCount { get; set; } = 3;

        [Required(ErrorMessage = "ExperienceReward is required")]
        [Range(0, 10000, ErrorMessage = "ExperienceReward must be between 0 and 10000")]
        public int ExperienceReward { get; set; } = 100;

        [Required(ErrorMessage = "GoldReward is required")]
        [Range(0, 5000, ErrorMessage = "GoldReward must be between 0 and 5000")]
        public int GoldReward { get; set; } = 50;

        [Required(ErrorMessage = "Status is required")]
        public QuestStatus Status { get; set; } = QuestStatus.Available;

        public List<Guid> RequiredItemIds { get; set; } = new();

        public List<string> RequiredEnemyTypes { get; set; } = new();

        [StringLength(50, ErrorMessage = "RequiredLocation cannot exceed 50 characters")]
        public string RequiredLocation { get; set; } = string.Empty;
    }
}