using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.Workflows;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowController : BaseApiController
    {
        private readonly IWorkflowService
            _workflowService;

        public WorkflowController(
            IWorkflowService workflowService)
        {
            _workflowService =
                workflowService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetWorkflowById(Guid id)
        {
            var workflow =
                await _workflowService
                    .GetWorkflowByIdAsync(id);

            if (workflow == null)
            {
                return ApiNotFound(
                    "Workflow not found");
            }

            return ApiOk(workflow);
        }

        [HttpGet("GetWorkflows")]
        public async Task<IActionResult>
            GetWorkflows(
                int pageNumber = 1,
                int pageSize = 10)
        {
            var result =
                await _workflowService
                    .GetWorkflowsAsync(
                        pageNumber,
                        pageSize);

            return ApiOk(result);
        }

        [HttpPost("CreateWorkflow")]
        public async Task<IActionResult>
            CreateWorkflow(
                [FromBody]
                CreateWorkflowRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            await _workflowService
                .CreateWorkflowAsync(dto);

            return ApiOk(
                "Workflow created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult>
            UpdateWorkflow(
                Guid id,
                [FromBody]
                UpdateWorkflowRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            await _workflowService
                .UpdateWorkflowAsync(
                    id,
                    dto);

            return ApiOk(
                "Workflow updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>
            DeleteWorkflow(Guid id)
        {
            await _workflowService
                .DeleteWorkflowAsync(id);

            return ApiOk(
                "Workflow deleted successfully");
        }
    }
}