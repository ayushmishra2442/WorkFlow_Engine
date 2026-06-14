using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Roles;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<RoleResponseDto>>
            GetRolesAsync()
        {
            _logger.LogInformation("Fetching all roles");

            var roles =
                await _roleRepository.GetRolesAsync();

            return roles.Select(
                role => new RoleResponseDto
                {
                    RoleId = role.RoleId,
                    Name = role.Name,
                    Description = role.Description,
                    IsActive = role.IsActive,
                    CreatedOn = role.CreatedOn
                });
        }

        public async Task<RoleResponseDto?>
            GetRoleByIdAsync(Guid roleId)
        {
            _logger.LogInformation(
                "Fetching role {RoleId}",
                roleId);

            var role =
                await _roleRepository
                    .GetRoleByIdAsync(roleId);

            if (role == null)
            {
                _logger.LogWarning(
                    "Role {RoleId} not found",
                    roleId);

                return null;
            }

            return new RoleResponseDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedOn = role.CreatedOn
            };
        }

        public async Task CreateRoleAsync(
            CreateRoleRequestDto dto)
        {
            _logger.LogInformation(
                "Creating role '{Name}'",
                dto.Name);

            await _roleRepository.CreateRoleAsync(dto);

            _logger.LogInformation(
                "Role '{Name}' created successfully",
                dto.Name);
        }

        public async Task UpdateRoleAsync(
            Guid roleId,
            UpdateRoleRequestDto dto)
        {
            _logger.LogInformation(
                "Updating role {RoleId}",
                roleId);

            await _roleRepository
                .UpdateRoleAsync(roleId, dto);
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            _logger.LogInformation(
                "Deleting role {RoleId}",
                roleId);

            await _roleRepository.DeleteRoleAsync(roleId);
        }
    }
}
