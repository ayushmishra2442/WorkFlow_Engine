namespace WorkflowManagement.Application.DTOs.UserRoles
{
    /// <summary>
    /// Returns a user with all their currently active roles.
    /// Used by GetRolesForUser.
    /// </summary>
    public class UserWithRolesResponseDto
    {
        public Guid UserId { get; set; }

        public string DisplayName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public IEnumerable<UserRoleResponseDto> Roles { get; set; }
            = Enumerable.Empty<UserRoleResponseDto>();
    }
}
