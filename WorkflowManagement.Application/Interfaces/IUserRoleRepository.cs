using WorkflowManagement.Application.DTOs.UserRoles;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Returns all active role assignments for a given user.
        /// </summary>
        Task<IEnumerable<UserRoleResponseDto>>
            GetRolesForUserAsync(Guid userId);

        /// <summary>
        /// Returns all active users assigned to a given role.
        /// </summary>
        Task<IEnumerable<UserRoleResponseDto>>
            GetUsersInRoleAsync(Guid roleId);

        Task AssignRoleToUserAsync(AssignRoleRequestDto dto);

        Task RemoveRoleFromUserAsync(Guid userRoleId);
    }
}
