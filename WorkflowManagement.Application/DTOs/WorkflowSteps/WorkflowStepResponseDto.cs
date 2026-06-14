namespace WorkflowManagement.Application.DTOs.WorkflowSteps
{
    public class WorkflowStepResponseDto
    {
        public Guid WorkflowStepId { get; set; }

        public Guid WorkflowId { get; set; }

        public Guid RoleId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public string StepName { get; set; }
            = string.Empty;

        public int StepOrder { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
