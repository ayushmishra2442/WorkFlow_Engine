using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.Workflows;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository
            _workflowRepository;

        private readonly IOrganizationRepository
            _organizationRepository;

        private readonly ILogger<WorkflowService>
            _logger;

        public WorkflowService(
            IWorkflowRepository workflowRepository,
            IOrganizationRepository organizationRepository,
            ILogger<WorkflowService> logger)
        {
            _workflowRepository =
                workflowRepository;

            _organizationRepository =
                organizationRepository;

            _logger = logger;
        }

        public async Task<PagedResponse<WorkflowResponseDto>>
            GetWorkflowsAsync(
                int pageNumber,
                int pageSize)
        {
            _logger.LogInformation(
                "Fetching workflows — Page {PageNumber}, Size {PageSize}",
                pageNumber,
                pageSize);

            var (items, totalCount) =
                await _workflowRepository
                    .GetWorkflowsAsync(
                        pageNumber,
                        pageSize);

            var dtos = items.Select(
                workflow =>
                    new WorkflowResponseDto
                    {
                        WorkflowId =
                            workflow.WorkflowId,

                        OrganizationId =
                            workflow.OrganizationId,

                        Name =
                            workflow.Name,

                        Description =
                            workflow.Description,

                        IsActive =
                            workflow.IsActive,

                        CreatedOn =
                            workflow.CreatedOn
                    });

            return new PagedResponse<WorkflowResponseDto>(
                dtos,
                pageNumber,
                pageSize,
                totalCount);
        }

        public async Task<WorkflowResponseDto?>
            GetWorkflowByIdAsync(
                Guid workflowId)
        {
            _logger.LogInformation(
                "Fetching workflow {WorkflowId}",
                workflowId);

            var workflow =
                await _workflowRepository
                    .GetWorkflowByIdAsync(
                        workflowId);

            if (workflow == null)
            {
                _logger.LogWarning(
                    "Workflow {WorkflowId} not found",
                    workflowId);

                return null;
            }

            return new WorkflowResponseDto
            {
                WorkflowId =
                    workflow.WorkflowId,

                OrganizationId =
                    workflow.OrganizationId,

                Name =
                    workflow.Name,

                Description =
                    workflow.Description,

                IsActive =
                    workflow.IsActive,

                CreatedOn =
                    workflow.CreatedOn
            };
        }

        public async Task CreateWorkflowAsync(
            CreateWorkflowRequestDto dto)
        {
            _logger.LogInformation(
                "Creating workflow '{Name}' for Organization {OrganizationId}",
                dto.Name,
                dto.OrganizationId);

            // Business validation: ensure OrganizationId exists
            var organization =
                await _organizationRepository
                    .GetOrganizationByIdAsync(
                        dto.OrganizationId);

            if (organization == null)
            {
                _logger.LogWarning(
                    "CreateWorkflow failed — Organization {OrganizationId} not found",
                    dto.OrganizationId);

                throw new KeyNotFoundException(
                    $"Organization with ID '{dto.OrganizationId}' was not found.");
            }

            await _workflowRepository
                .CreateWorkflowAsync(dto);

            _logger.LogInformation(
                "Workflow '{Name}' created successfully",
                dto.Name);
        }

        public async Task UpdateWorkflowAsync(
            Guid workflowId,
            UpdateWorkflowRequestDto dto)
        {
            _logger.LogInformation(
                "Updating workflow {WorkflowId}",
                workflowId);

            await _workflowRepository
                .UpdateWorkflowAsync(
                    workflowId,
                    dto);
        }

        public async Task DeleteWorkflowAsync(
            Guid workflowId)
        {
            _logger.LogInformation(
                "Deleting workflow {WorkflowId}",
                workflowId);

            await _workflowRepository
                .DeleteWorkflowAsync(workflowId);
        }
    }
}