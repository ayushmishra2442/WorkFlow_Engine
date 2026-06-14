namespace WorkflowManagement.Application.DTOs.Roles
{
    public class RoleResponseDto
    {
        public Guid RoleId { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
