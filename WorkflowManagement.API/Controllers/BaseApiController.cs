using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkflowManagement.API.Models;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaseApiController : ControllerBase
    {


        protected IActionResult ApiOk<T>(T data)
        {
            return Ok(
                new ApiResponse<T>(data));
        }

        /// <summary>
        /// Standard error response
        /// </summary>
        protected IActionResult ApiError(
            string message,
            int statusCode = 400)
        {
            return StatusCode(
                statusCode,
                new ApiResponse<object>(
                    message));
        }

        /// <summary>
        /// Standard not found response
        /// </summary>
        protected IActionResult ApiNotFound(
            string message =
                "Resource not found")
        {
            return NotFound(
                new ApiResponse<object>(
                    message));
        }



    }
}
