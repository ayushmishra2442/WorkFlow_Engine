using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Users
{
    public class CreateUserRequestDto
    {
        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; }
            = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(320)]
        public string Email { get; set; }
            = string.Empty;
    }
}
