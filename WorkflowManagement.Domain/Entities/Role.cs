namespace WorkflowManagement.Domain.Entities
{
    public class Role
    {
        public Guid RoleId { get; set; }

        public string Name { get; set; }
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
