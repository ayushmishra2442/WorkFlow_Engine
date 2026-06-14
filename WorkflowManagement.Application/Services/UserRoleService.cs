using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.UserRoles;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository
            _userRoleRepository;

        private readonly IUserRepository
            _userRepository;

        private readonly IRoleRepository
            _roleRepository;

        private readonly ILogger<UserRoleService>
            _logger;

        public UserRoleService(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ILogger<UserRoleService> logger)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<UserWithRolesResponseDto?>
            GetRolesForUserAsync(Guid userId)
        {
            _logger.LogInformation(
                "Fetching roles for User {UserId}",
                userId);

            var user =
                await _userRepository
                    .GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning(
                    "GetRolesForUser — User {UserId} not found",
                    userId);

                return null;
            }

            var roles =
                await _userRoleRepository
                    .GetRolesForUserAsync(userId);

            return new UserWithRolesResponseDto
            {
                UserId = user.UserId,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<IEnumerable<UserRoleResponseDto>>
            GetUsersInRoleAsync(Guid roleId)
        {
            _logger.LogInformation(
                "Fetching users in Role {RoleId}",
                roleId);

            return await _userRoleRepository
                .GetUsersInRoleAsync(roleId);
        }

        public async Task AssignRoleToUserAsync(
            AssignRoleRequestDto dto)
        {
            _logger.LogInformation(
                "Assigning Role {RoleId} to User {UserId}",
                dto.RoleId,
                dto.UserId);

            // Business validation: User must exist
            var user =
                await _userRepository
                    .GetUserByIdAsync(dto.UserId);

            if (user == null)
            {
                _logger.LogWarning(
                    "AssignRole failed — User {UserId} not found",
                    dto.UserId);

                throw new KeyNotFoundException(
                    $"User with ID '{dto.UserId}' was not found.");
            }

            // Business validation: Role must exist
            var role =
                await _roleRepository
                    .GetRoleByIdAsync(dto.RoleId);

            if (role == null)
            {
                _logger.LogWarning(
                    "AssignRole failed — Role {RoleId} not found",
                    dto.RoleId);

                throw new KeyNotFoundException(
                    $"Role with ID '{dto.RoleId}' was not found.");
            }

            await _userRoleRepository
                .AssignRoleToUserAsync(dto);

            _logger.LogInformation(
                "Role {RoleId} assigned to User {UserId} successfully",
                dto.RoleId,
                dto.UserId);
        }

        public async Task RemoveRoleFromUserAsync(
            Guid userRoleId)
        {
            _logger.LogInformation(
                "Removing UserRole {UserRoleId}",
                userRoleId);

            await _userRoleRepository
                .RemoveRoleFromUserAsync(userRoleId);
        }
    }
}
