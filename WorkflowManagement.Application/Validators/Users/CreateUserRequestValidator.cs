using FluentValidation;
using WorkflowManagement.Application.DTOs.Users;

namespace WorkflowManagement.Application.Validators.Users
{
    public class CreateUserRequestValidator
        : AbstractValidator<CreateUserRequestDto>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("OrganizationId is required.");

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
