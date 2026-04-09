using FluentValidation;
using RpgGame.Core.DTOs;

namespace RpgGame.Core.Validators
{
    public class CreateGameLocationDtoValidator : AbstractValidator<CreateGameLocationDto>
    {
        public CreateGameLocationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Location name is required")
                .Length(2, 50).WithMessage("Name must be between 2 and 50 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Location description is required")
                .Length(10, 300).WithMessage("Description must be between 10 and 300 characters");

            RuleFor(x => x.RequiredLevel)
                .GreaterThanOrEqualTo(1).WithMessage("Required level must be at least 1")
                .LessThanOrEqualTo(100).WithMessage("Required level cannot exceed 100");

            RuleFor(x => x.AvailableEnemies)
                .NotNull().WithMessage("Available enemies list cannot be null");

            RuleFor(x => x.AvailableQuests)
                .NotNull().WithMessage("Available quests list cannot be null");
        }
    }
}