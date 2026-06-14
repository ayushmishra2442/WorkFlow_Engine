using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.Users;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository
            _userRepository;

        private readonly IOrganizationRepository
            _organizationRepository;

        private readonly ILogger<UserService>
            _logger;

        public UserService(
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;

            _organizationRepository =
                organizationRepository;

            _logger = logger;
        }

        public async Task<PagedResponse<UserResponseDto>>
            GetUsersAsync(
                int pageNumber,
                int pageSize)
        {
            _logger.LogInformation(
                "Fetching users — Page {PageNumber}, Size {PageSize}",
                pageNumber,
                pageSize);

            var (items, totalCount) =
                await _userRepository.GetUsersAsync(
                    pageNumber,
                    pageSize);

            var dtos = items.Select(
                user => new UserResponseDto
                {
                    UserId = user.UserId,
                    OrganizationId = user.OrganizationId,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn
                });

            return new PagedResponse<UserResponseDto>(
                dtos,
                pageNumber,
                pageSize,
                totalCount);
        }

        public async Task<UserResponseDto?>
            GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation(
                "Fetching user {UserId}",
                userId);

            var user =
                await _userRepository
                    .GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning(
                    "User {UserId} not found",
                    userId);

                return null;
            }

            return new UserResponseDto
            {
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                DisplayName = user.DisplayName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn
            };
        }

        public async Task CreateUserAsync(
            CreateUserRequestDto dto)
        {
            _logger.LogInformation(
                "Creating user '{Email}' in Organization {OrganizationId}",
                dto.Email,
                dto.OrganizationId);

            // Business validation: ensure OrganizationId exists
            var organization =
                await _organizationRepository
                    .GetOrganizationByIdAsync(
                        dto.OrganizationId);

            if (organization == null)
            {
                _logger.LogWarning(
                    "CreateUser failed — Organization {OrganizationId} not found",
                    dto.OrganizationId);

                throw new KeyNotFoundException(
                    $"Organization with ID '{dto.OrganizationId}' was not found.");
            }

            await _userRepository.CreateUserAsync(dto);

            _logger.LogInformation(
                "User '{Email}' created successfully",
                dto.Email);
        }

        public async Task UpdateUserAsync(
            Guid userId,
            UpdateUserRequestDto dto)
        {
            _logger.LogInformation(
                "Updating user {UserId}",
                userId);

            await _userRepository
                .UpdateUserAsync(userId, dto);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            _logger.LogInformation(
                "Deleting user {UserId}",
                userId);

            await _userRepository.DeleteUserAsync(userId);
        }
    }
}
