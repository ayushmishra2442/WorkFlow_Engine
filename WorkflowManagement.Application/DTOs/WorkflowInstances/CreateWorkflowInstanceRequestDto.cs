using System;

namespace WorkflowManagement.Application.DTOs.WorkflowInstances
{
    public class CreateWorkflowInstanceRequestDto
    {
        public Guid WorkflowId { get; set; }
        public Guid InitiatedByUserId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
