using WorkflowManagement.Application.DTOs.Roles;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetRolesAsync();

        Task<Role?> GetRoleByIdAsync(Guid roleId);

        Task CreateRoleAsync(CreateRoleRequestDto dto);

        Task UpdateRoleAsync(
            Guid roleId,
            UpdateRoleRequestDto dto);

        Task DeleteRoleAsync(Guid roleId);
    }
}
