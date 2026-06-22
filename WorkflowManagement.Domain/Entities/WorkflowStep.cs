namespace WorkflowManagement.Domain.Entities
{
    public class WorkflowStep
    {
        public Guid WorkflowStepId { get; set; }

        public Guid WorkflowId { get; set; }

        /// <summary>
        /// The role responsible for actioning this step.
        /// Actual user is resolved at runtime via WorkflowTask.
        /// Optional if RoutingType is DirectManager.
        /// </summary>
        public Guid? RoleId { get; set; }

        public string RoutingType { get; set; }
            = "Role";

        public string StepName { get; set; }
            = string.Empty;

        public int StepOrder { get; set; }

        /// <summary>
        /// Populated from JOIN with auth.Roles in read SPs.
        /// Not stored in this table.
        /// </summary>
        public string RoleName { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public Guid? ModifiedBy { get; set; }

        public bool DeleteFlag { get; set; }
    }
}
