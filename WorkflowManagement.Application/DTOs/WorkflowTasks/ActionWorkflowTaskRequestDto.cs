using System;

namespace WorkflowManagement.Application.DTOs.WorkflowTasks
{
    public class ActionWorkflowTaskRequestDto
    {
        public Guid WorkflowTaskId { get; set; }
        public Guid ActionedByUserId { get; set; }
        public string Status { get; set; } = string.Empty; // "Approved" or "Rejected"
        public string? Comments { get; set; }
    }
}
