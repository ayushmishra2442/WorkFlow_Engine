namespace WorkflowManagement.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public string DisplayName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        /// <summary>
        /// Azure Entra ID object ID (oid claim).
        /// Populated during Phase 6 SSO integration.
        /// </summary>
        public string? AzureObjectId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public Guid? ModifiedBy { get; set; }

        public bool DeleteFlag { get; set; }
    }
}
