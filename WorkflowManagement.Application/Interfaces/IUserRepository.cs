using WorkflowManagement.Application.DTOs.Users;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<(IEnumerable<User> Items, int TotalCount)>
            GetUsersAsync(
                int pageNumber,
                int pageSize);

        Task<User?> GetUserByIdAsync(Guid userId);

        Task<User?> GetUserByEmailAsync(string email);

        Task CreateUserAsync(CreateUserRequestDto dto);

        Task UpdateUserAsync(
            Guid userId,
            UpdateUserRequestDto dto);

        Task DeleteUserAsync(Guid userId);
    }
}
