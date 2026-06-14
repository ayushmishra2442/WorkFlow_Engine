using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.UserRoles
{
    public class AssignRoleRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}
