using WorkflowManagement.Application.DTOs.Roles;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDto>> GetRolesAsync();

        Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId);

        Task CreateRoleAsync(CreateRoleRequestDto dto);

        Task UpdateRoleAsync(
            Guid roleId,
            UpdateRoleRequestDto dto);

        Task DeleteRoleAsync(Guid roleId);
    }
}
