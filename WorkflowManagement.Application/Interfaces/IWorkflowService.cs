using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.Workflows;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowService
    {
        Task<WorkflowResponseDto?>
            GetWorkflowByIdAsync(
                Guid workflowId);

        Task<PagedResponse<WorkflowResponseDto>>
            GetWorkflowsAsync(
                int pageNumber,
                int pageSize);

        Task CreateWorkflowAsync(
            CreateWorkflowRequestDto dto);

        Task UpdateWorkflowAsync(
            Guid workflowId,
            UpdateWorkflowRequestDto dto);

        Task DeleteWorkflowAsync(
            Guid workflowId);
    }
}