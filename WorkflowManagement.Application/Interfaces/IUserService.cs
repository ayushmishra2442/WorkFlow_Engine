using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.Users;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponseDto>> GetUsersAsync(
            int pageNumber,
            int pageSize);

        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);

        Task CreateUserAsync(CreateUserRequestDto dto);

        Task UpdateUserAsync(
            Guid userId,
            UpdateUserRequestDto dto);

        Task DeleteUserAsync(Guid userId);
    }
}
