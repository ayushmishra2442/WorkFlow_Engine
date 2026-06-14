using FluentValidation;
using WorkflowManagement.Application.DTOs.WorkflowSteps;

namespace WorkflowManagement.Application.Validators.WorkflowSteps
{
    public class CreateWorkflowStepRequestValidator
        : AbstractValidator<CreateWorkflowStepRequestDto>
    {
        public CreateWorkflowStepRequestValidator()
        {
            RuleFor(x => x.WorkflowId)
                .NotEmpty()
                .WithMessage("WorkflowId is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("RoleId is required.");

            RuleFor(x => x.StepName)
                .NotEmpty()
                .WithMessage("Step name is required.")
                .MaximumLength(200)
                .WithMessage("Step name cannot exceed 200 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Step name cannot be whitespace.");

            RuleFor(x => x.StepOrder)
                .GreaterThan(0)
                .WithMessage("StepOrder must be a positive integer.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);
        }
    }
}
