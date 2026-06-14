using FluentValidation;
using WorkflowManagement.Application.DTOs.Roles;

namespace WorkflowManagement.Application.Validators.Roles
{
    public class CreateRoleRequestValidator
        : AbstractValidator<CreateRoleRequestDto>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Role name is required.")
                .MaximumLength(100)
                .WithMessage("Role name cannot exceed 100 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Role name cannot be whitespace.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);
        }
    }
}
