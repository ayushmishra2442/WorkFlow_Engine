using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.WorkflowSteps;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowStepController
        : BaseApiController
    {
        private readonly IWorkflowStepService
            _workflowStepService;

        public WorkflowStepController(
            IWorkflowStepService workflowStepService)
        {
            _workflowStepService =
                workflowStepService;
        }

        /// <summary>
        /// Returns all steps for a workflow, ordered by StepOrder.
        /// </summary>
        [HttpGet("GetSteps/{workflowId}")]
        public async Task<IActionResult>
            GetWorkflowSteps(Guid workflowId)
        {
            var steps =
                await _workflowStepService
                    .GetWorkflowStepsAsync(workflowId);

            return ApiOk(steps);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetWorkflowStepById(Guid id)
        {
            var step =
                await _workflowStepService
                    .GetWorkflowStepByIdAsync(id);

            if (step == null)
            {
                return ApiNotFound(
                    "Workflow step not found");
            }

            return ApiOk(step);
        }

        [HttpPost("CreateStep")]
        public async Task<IActionResult>
            CreateWorkflowStep(
                [FromBody]
                CreateWorkflowStepRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _workflowStepService
                .CreateWorkflowStepAsync(dto);

            return ApiOk(
                "Workflow step created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult>
            UpdateWorkflowStep(
                Guid id,
                [FromBody]
                UpdateWorkflowStepRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _workflowStepService
                .UpdateWorkflowStepAsync(id, dto);

            return ApiOk(
                "Workflow step updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>
            DeleteWorkflowStep(Guid id)
        {
            await _workflowStepService
                .DeleteWorkflowStepAsync(id);

            return ApiOk(
                "Workflow step deleted successfully");
        }
    }
}
