using System;

namespace WorkflowManagement.Domain.Entities
{
    public class WorkflowTask
    {
        public Guid WorkflowTaskId { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string WorkflowInstanceTitle { get; set; } = string.Empty;
        public Guid WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public Guid InitiatedByUserId { get; set; }
        public string InitiatedByUserName { get; set; } = string.Empty;
        
        public Guid WorkflowStepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        
        public Guid AssignedToRoleId { get; set; }
        public string AssignedToRoleName { get; set; } = string.Empty;
        public Guid? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Pending";
        public string? Comments { get; set; }
        
        public DateTime AssignedOn { get; set; }
        public DateTime? ActionedOn { get; set; }
        public Guid? ActionedByUserId { get; set; }
        public string ActionedByUserName { get; set; } = string.Empty;
    }
}
