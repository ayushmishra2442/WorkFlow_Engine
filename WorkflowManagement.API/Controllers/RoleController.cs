using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.Roles;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseApiController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetRoleById(Guid id)
        {
            var role =
                await _roleService
                    .GetRoleByIdAsync(id);

            if (role == null)
            {
                return ApiNotFound("Role not found");
            }

            return ApiOk(role);
        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult>
            GetRoles()
        {
            var roles =
                await _roleService.GetRolesAsync();

            return ApiOk(roles);
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult>
            CreateRole(
                [FromBody]
                CreateRoleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _roleService.CreateRoleAsync(dto);

            return ApiOk(
                "Role created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult>
            UpdateRole(
                Guid id,
                [FromBody]
                UpdateRoleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _roleService
                .UpdateRoleAsync(id, dto);

            return ApiOk(
                "Role updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>
            DeleteRole(Guid id)
        {
            await _roleService.DeleteRoleAsync(id);

            return ApiOk(
                "Role deleted successfully");
        }
    }
}
