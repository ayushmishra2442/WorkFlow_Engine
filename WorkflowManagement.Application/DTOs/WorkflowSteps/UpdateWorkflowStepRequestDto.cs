using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.WorkflowSteps
{
    public class UpdateWorkflowStepRequestDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StepName { get; set; }
            = string.Empty;

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "StepOrder must be a positive integer.")]
        public int StepOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
