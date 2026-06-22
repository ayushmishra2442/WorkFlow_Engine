using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.WorkflowSteps;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Application.Services
{
    public class WorkflowStepService : IWorkflowStepService
    {
        private readonly IWorkflowStepRepository
            _workflowStepRepository;

        private readonly IWorkflowRepository
            _workflowRepository;

        private readonly IRoleRepository
            _roleRepository;

        private readonly ILogger<WorkflowStepService>
            _logger;

        public WorkflowStepService(
            IWorkflowStepRepository workflowStepRepository,
            IWorkflowRepository workflowRepository,
            IRoleRepository roleRepository,
            ILogger<WorkflowStepService> logger)
        {
            _workflowStepRepository =
                workflowStepRepository;

            _workflowRepository =
                workflowRepository;

            _roleRepository =
                roleRepository;

            _logger = logger;
        }

        public async Task<IEnumerable<WorkflowStepResponseDto>>
            GetWorkflowStepsAsync(Guid workflowId)
        {
            _logger.LogInformation(
                "Fetching steps for Workflow {WorkflowId}",
                workflowId);

            var steps =
                await _workflowStepRepository
                    .GetWorkflowStepsAsync(workflowId);

            return steps.Select(
                step => new WorkflowStepResponseDto
                {
                    WorkflowStepId = step.WorkflowStepId,
                    WorkflowId = step.WorkflowId,
                    RoleId = step.RoleId,
                    RoleName = step.RoleName,
                    RoutingType = step.RoutingType,
                    StepName = step.StepName,
                    StepOrder = step.StepOrder,
                    Description = step.Description,
                    IsActive = step.IsActive,
                    CreatedOn = step.CreatedOn
                });
        }

        public async Task<WorkflowStepResponseDto?>
            GetWorkflowStepByIdAsync(Guid workflowStepId)
        {
            _logger.LogInformation(
                "Fetching workflow step {WorkflowStepId}",
                workflowStepId);

            var step =
                await _workflowStepRepository
                    .GetWorkflowStepByIdAsync(
                        workflowStepId);

            if (step == null)
            {
                _logger.LogWarning(
                    "WorkflowStep {WorkflowStepId} not found",
                    workflowStepId);

                return null;
            }

            return new WorkflowStepResponseDto
            {
                WorkflowStepId = step.WorkflowStepId,
                WorkflowId = step.WorkflowId,
                RoleId = step.RoleId,
                RoleName = step.RoleName,
                RoutingType = step.RoutingType,
                StepName = step.StepName,
                StepOrder = step.StepOrder,
                Description = step.Description,
                IsActive = step.IsActive,
                CreatedOn = step.CreatedOn
            };
        }

        public async Task CreateWorkflowStepAsync(
            CreateWorkflowStepRequestDto dto)
        {
            _logger.LogInformation(
                "Creating step '{StepName}' (Order {StepOrder}) for Workflow {WorkflowId}",
                dto.StepName,
                dto.StepOrder,
                dto.WorkflowId);

            // Business validation: Workflow must exist
            var workflow =
                await _workflowRepository
                    .GetWorkflowByIdAsync(dto.WorkflowId);

            if (workflow == null)
            {
                _logger.LogWarning(
                    "CreateWorkflowStep failed — Workflow {WorkflowId} not found",
                    dto.WorkflowId);

                throw new KeyNotFoundException(
                    $"Workflow with ID '{dto.WorkflowId}' was not found.");
            }

            // Business validation: Role must exist (if provided)
            if (dto.RoleId.HasValue)
            {
                var role =
                    await _roleRepository
                        .GetRoleByIdAsync(dto.RoleId.Value);

                if (role == null)
                {
                    _logger.LogWarning(
                        "CreateWorkflowStep failed — Role {RoleId} not found",
                        dto.RoleId);

                    throw new KeyNotFoundException(
                        $"Role with ID '{dto.RoleId}' was not found.");
                }
            }
            else if (dto.RoutingType == "Role")
            {
                throw new ArgumentException("RoleId is required when RoutingType is 'Role'.");
            }

            await _workflowStepRepository
                .CreateWorkflowStepAsync(dto);

            _logger.LogInformation(
                "WorkflowStep '{StepName}' created for Workflow {WorkflowId}",
                dto.StepName,
                dto.WorkflowId);
        }

        public async Task UpdateWorkflowStepAsync(
            Guid workflowStepId,
            UpdateWorkflowStepRequestDto dto)
        {
            _logger.LogInformation(
                "Updating workflow step {WorkflowStepId}",
                workflowStepId);

            // Business validation: Role must exist (if provided)
            if (dto.RoleId.HasValue)
            {
                var role =
                    await _roleRepository
                        .GetRoleByIdAsync(dto.RoleId.Value);

                if (role == null)
                {
                    throw new KeyNotFoundException(
                        $"Role with ID '{dto.RoleId}' was not found.");
                }
            }
            else if (dto.RoutingType == "Role")
            {
                throw new ArgumentException("RoleId is required when RoutingType is 'Role'.");
            }

            await _workflowStepRepository
                .UpdateWorkflowStepAsync(
                    workflowStepId,
                    dto);
        }

        public async Task DeleteWorkflowStepAsync(
            Guid workflowStepId)
        {
            _logger.LogInformation(
                "Deleting workflow step {WorkflowStepId}",
                workflowStepId);

            await _workflowStepRepository
                .DeleteWorkflowStepAsync(workflowStepId);
        }
    }
}
