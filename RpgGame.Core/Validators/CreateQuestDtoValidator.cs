using FluentValidation;
using RpgGame.Core.DTOs;

namespace RpgGame.Core.Validators
{
    public class CreateQuestDtoValidator : AbstractValidator<CreateQuestDto>
    {
        public CreateQuestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Quest title is required")
                .Length(5, 100).WithMessage("Title must be between 5 and 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Quest description is required")
                .Length(10, 500).WithMessage("Description must be between 10 and 500 characters");

            RuleFor(x => x.Objective)
                .NotEmpty().WithMessage("Quest objective is required")
                .Length(5, 200).WithMessage("Objective must be between 5 and 200 characters");

            RuleFor(x => x.TargetCount)
                .GreaterThan(0).WithMessage("Target count must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Target count cannot exceed 100");

            RuleFor(x => x.ExperienceReward)
                .GreaterThanOrEqualTo(0).WithMessage("Experience reward cannot be negative");

            RuleFor(x => x.GoldReward)
                .GreaterThanOrEqualTo(0).WithMessage("Gold reward cannot be negative");
        }
    }
}