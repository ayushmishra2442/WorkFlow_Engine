using FluentValidation;
using WorkflowManagement.Application.DTOs.Users;

namespace WorkflowManagement.Application.Validators.Users
{
    public class UpdateUserRequestValidator
        : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Display name is required.")
                .MaximumLength(200)
                .WithMessage("Display name cannot exceed 200 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Display name cannot be whitespace.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email must be a valid email address.")
                .MaximumLength(320)
                .WithMessage("Email cannot exceed 320 characters.");
        }
    }
}
