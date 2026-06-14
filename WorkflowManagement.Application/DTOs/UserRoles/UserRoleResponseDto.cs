namespace WorkflowManagement.Application.DTOs.UserRoles
{
    public class UserRoleResponseDto
    {
        public Guid UserRoleId { get; set; }

        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public DateTime AssignedOn { get; set; }
    }
}
