using System;

namespace WorkflowManagement.Application.DTOs.WorkflowInstances
{
    public class WorkflowInstanceResponseDto
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public Guid InitiatedByUserId { get; set; }
        public string InitiatedByUserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
