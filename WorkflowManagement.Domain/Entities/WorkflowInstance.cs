using System;

namespace WorkflowManagement.Domain.Entities
{
    public class WorkflowInstance
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public Guid InitiatedByUserId { get; set; }
        public string InitiatedByUserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
