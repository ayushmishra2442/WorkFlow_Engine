using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.UserRoles;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : BaseApiController
    {
        private readonly IUserRoleService
            _userRoleService;

        public UserRoleController(
            IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        /// <summary>
        /// Returns all roles assigned to a specific user.
        /// </summary>
        [HttpGet("GetRolesForUser/{userId}")]
        public async Task<IActionResult>
            GetRolesForUser(Guid userId)
        {
            var result =
                await _userRoleService
                    .GetRolesForUserAsync(userId);

            if (result == null)
            {
                return ApiNotFound("User not found");
            }

            return ApiOk(result);
        }

        /// <summary>
        /// Returns all users assigned to a specific role.
        /// </summary>
        [HttpGet("GetUsersInRole/{roleId}")]
        public async Task<IActionResult>
            GetUsersInRole(Guid roleId)
        {
            var result =
                await _userRoleService
                    .GetUsersInRoleAsync(roleId);

            return ApiOk(result);
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        [HttpPost("AssignRole")]
        public async Task<IActionResult>
            AssignRole(
                [FromBody]
                AssignRoleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userRoleService
                .AssignRoleToUserAsync(dto);

            return ApiOk(
                "Role assigned successfully");
        }

        /// <summary>
        /// Removes a role assignment by UserRoleId.
        /// </summary>
        [HttpDelete("{userRoleId}")]
        public async Task<IActionResult>
            RemoveRole(Guid userRoleId)
        {
            await _userRoleService
                .RemoveRoleFromUserAsync(userRoleId);

            return ApiOk(
                "Role removed successfully");
        }
    }
}
