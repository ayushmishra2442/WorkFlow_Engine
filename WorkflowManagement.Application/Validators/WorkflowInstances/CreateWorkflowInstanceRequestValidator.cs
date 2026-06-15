using FluentValidation;
using WorkflowManagement.Application.DTOs.WorkflowInstances;

namespace WorkflowManagement.Application.Validators.WorkflowInstances
{
    public class CreateWorkflowInstanceRequestValidator : AbstractValidator<CreateWorkflowInstanceRequestDto>
    {
        public CreateWorkflowInstanceRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Workflow instance title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Title cannot be whitespace.");

            RuleFor(x => x.WorkflowId)
                .NotEmpty().WithMessage("Workflow ID is required.");

            RuleFor(x => x.InitiatedByUserId)
                .NotEmpty().WithMessage("Initiator User ID is required.");
        }
    }
}
