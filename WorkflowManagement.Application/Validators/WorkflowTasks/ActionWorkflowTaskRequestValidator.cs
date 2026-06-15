using FluentValidation;
using WorkflowManagement.Application.DTOs.WorkflowTasks;

namespace WorkflowManagement.Application.Validators.WorkflowTasks
{
    public class ActionWorkflowTaskRequestValidator : AbstractValidator<ActionWorkflowTaskRequestDto>
    {
        public ActionWorkflowTaskRequestValidator()
        {
            RuleFor(x => x.WorkflowTaskId)
                .NotEmpty().WithMessage("Workflow Task ID is required.");

            RuleFor(x => x.ActionedByUserId)
                .NotEmpty().WithMessage("Actioned By User ID is required.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Task status is required.")
                .Must(status => status == "Approved" || status == "Rejected")
                .WithMessage("Task status must be either 'Approved' or 'Rejected'.");

            RuleFor(x => x.Comments)
                .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters.");
        }
    }
}
