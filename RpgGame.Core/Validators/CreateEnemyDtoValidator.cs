using FluentValidation;
using RpgGame.Core.DTOs;

namespace RpgGame.Core.Validators
{
    public class CreateEnemyDtoValidator : AbstractValidator<CreateEnemyDto>
    {
        public CreateEnemyDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Enemy name is required")
                .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");

            RuleFor(x => x.Level)
                .GreaterThan(0).WithMessage("Level must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Level cannot exceed 100");

            RuleFor(x => x.Health)
                .GreaterThan(0).WithMessage("Health must be greater than 0");

            RuleFor(x => x.MaxHealth)
                .GreaterThanOrEqualTo(x => x.Health).WithMessage("Max health must be greater than or equal to current health");

            RuleFor(x => x.Attack)
                .GreaterThan(0).WithMessage("Attack must be greater than 0");

            RuleFor(x => x.ExperienceReward)
                .GreaterThanOrEqualTo(0).WithMessage("Experience reward cannot be negative");

            RuleFor(x => x.GoldReward)
                .GreaterThanOrEqualTo(0).WithMessage("Gold reward cannot be negative");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required");
        }
    }
}