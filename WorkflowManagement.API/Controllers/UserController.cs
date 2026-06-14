using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.Users;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetUserById(Guid id)
        {
            var user =
                await _userService
                    .GetUserByIdAsync(id);

            if (user == null)
            {
                return ApiNotFound("User not found");
            }

            return ApiOk(user);
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult>
            GetUsers(
                int pageNumber = 1,
                int pageSize = 10)
        {
            var result =
                await _userService.GetUsersAsync(
                    pageNumber,
                    pageSize);

            return ApiOk(result);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult>
            CreateUser(
                [FromBody]
                CreateUserRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.CreateUserAsync(dto);

            return ApiOk(
                "User created successfully");
        }

        

        [HttpPut("{id}")]
        public async Task<IActionResult>
            UpdateUser(
                Guid id,
                [FromBody]
                UpdateUserRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService
                .UpdateUserAsync(id, dto);

            return ApiOk(
                "User updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>
            DeleteUser(Guid id)
        {
            await _userService.DeleteUserAsync(id);

            return ApiOk(
                "User deleted successfully");
        }
    }
}
