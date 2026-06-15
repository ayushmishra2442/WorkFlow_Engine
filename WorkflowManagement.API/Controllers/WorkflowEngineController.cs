using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.WorkflowInstances;
using WorkflowManagement.Application.DTOs.WorkflowTasks;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowEngineController : BaseApiController
    {
        private readonly IWorkflowEngineService _engineService;

        public WorkflowEngineController(IWorkflowEngineService engineService)
        {
            _engineService = engineService;
        }

        [HttpPost("Start")]
        public async Task<IActionResult> StartWorkflowInstance([FromBody] CreateWorkflowInstanceRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _engineService.StartWorkflowInstanceAsync(dto);
            return ApiOk(result);
        }

        [HttpGet("Instance/{id}")]
        public async Task<IActionResult> GetWorkflowInstanceById(Guid id)
        {
            var instance = await _engineService.GetWorkflowInstanceByIdAsync(id);
            if (instance == null)
            {
                return ApiNotFound($"Workflow instance with ID '{id}' was not found.");
            }

            return ApiOk(instance);
        }

        [HttpGet("Instances")]
        public async Task<IActionResult> GetWorkflowInstances(
            [FromQuery] string? status,
            [FromQuery] Guid? workflowId,
            [FromQuery] Guid? initiatedByUserId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _engineService.GetWorkflowInstancesAsync(
                status,
                workflowId,
                initiatedByUserId,
                pageNumber,
                pageSize);

            return ApiOk(result);
        }

        [HttpGet("Tasks/MyTasks/{userId}")]
        public async Task<IActionResult> GetMyTasks(Guid userId)
        {
            var tasks = await _engineService.GetMyTasksAsync(userId);
            return ApiOk(tasks);
        }

        [HttpGet("Tasks/InstanceHistory/{instanceId}")]
        public async Task<IActionResult> GetTasksByInstance(Guid instanceId)
        {
            var tasks = await _engineService.GetTasksByInstanceAsync(instanceId);
            return ApiOk(tasks);
        }

        [HttpPost("Tasks/Action")]
        public async Task<IActionResult> ActionTask([FromBody] ActionWorkflowTaskRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _engineService.ActionTaskAsync(dto);
            return ApiOk("Task action completed successfully.");
        }

        [HttpPost("Instance/Cancel")]
        public async Task<IActionResult> CancelWorkflowInstance([FromQuery] Guid instanceId, [FromQuery] Guid actionedByUserId)
        {
            await _engineService.CancelWorkflowInstanceAsync(instanceId, actionedByUserId);
            return ApiOk("Workflow instance cancelled successfully.");
        }
    }
}
