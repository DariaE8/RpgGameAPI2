using FluentValidation;
using RpgGame.Core.DTOs;

namespace RpgGame.Core.Validators
{
    public class CreatePlayerDtoValidator : AbstractValidator<CreatePlayerDto>
    {
        public CreatePlayerDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long")
                .MaximumLength(30).WithMessage("Name cannot exceed 30 characters");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email address is required");
        }
    }
}
