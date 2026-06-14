using FluentValidation;
using WorkflowManagement.Application.DTOs.UserRoles;

namespace WorkflowManagement.Application.Validators.UserRoles
{
    public class AssignRoleRequestValidator
        : AbstractValidator<AssignRoleRequestDto>
    {
        public AssignRoleRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("RoleId is required.");
        }
    }
}
