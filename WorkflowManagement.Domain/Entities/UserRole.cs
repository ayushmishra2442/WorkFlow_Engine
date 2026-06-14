namespace WorkflowManagement.Domain.Entities
{
    public class UserRole
    {
        public Guid UserRoleId { get; set; }

        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public DateTime AssignedOn { get; set; }

        public Guid? AssignedBy { get; set; }

        public bool IsActive { get; set; }

        public bool DeleteFlag { get; set; }
    }
}
