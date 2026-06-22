using FluentValidation;
using WorkflowManagement.Application.DTOs.WorkflowSteps;

namespace WorkflowManagement.Application.Validators.WorkflowSteps
{
    public class UpdateWorkflowStepRequestValidator
        : AbstractValidator<UpdateWorkflowStepRequestDto>
    {
        public UpdateWorkflowStepRequestValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("RoleId is required when RoutingType is 'Role'.")
                .When(x => x.RoutingType == "Role");

            RuleFor(x => x.RoutingType)
                .NotEmpty()
                .WithMessage("RoutingType is required.")
                .Must(type => type == "Role" || type == "DirectManager")
                .WithMessage("RoutingType must be either 'Role' or 'DirectManager'.");

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
