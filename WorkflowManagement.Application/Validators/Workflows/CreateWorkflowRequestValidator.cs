using FluentValidation;
using WorkflowManagement.Application.DTOs.Workflows;

namespace WorkflowManagement.Application.Validators.Workflows
{
    public class CreateWorkflowRequestValidator
        : AbstractValidator<CreateWorkflowRequestDto>
    {
        public CreateWorkflowRequestValidator()
        {
            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .WithMessage("OrganizationId is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Workflow name is required.")
                .MaximumLength(200)
                .WithMessage("Workflow name cannot exceed 200 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Workflow name cannot be whitespace.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);
        }
    }
}
