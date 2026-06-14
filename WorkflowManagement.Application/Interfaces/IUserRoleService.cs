using WorkflowManagement.Application.DTOs.UserRoles;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<UserWithRolesResponseDto?>
            GetRolesForUserAsync(Guid userId);

        Task<IEnumerable<UserRoleResponseDto>>
            GetUsersInRoleAsync(Guid roleId);

        Task AssignRoleToUserAsync(AssignRoleRequestDto dto);

        Task RemoveRoleFromUserAsync(Guid userRoleId);
    }
}
