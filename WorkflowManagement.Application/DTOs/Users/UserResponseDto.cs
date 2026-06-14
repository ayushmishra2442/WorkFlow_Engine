namespace WorkflowManagement.Application.DTOs.Users
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public string DisplayName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
