using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.Roles
{
    public class CreateRoleRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
            = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
