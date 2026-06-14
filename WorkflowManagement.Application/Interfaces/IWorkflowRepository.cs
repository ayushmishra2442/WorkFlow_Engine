using WorkflowManagement.Application.DTOs.Workflows;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<(IEnumerable<Workflow> Items, int TotalCount)>
            GetWorkflowsAsync(
                int pageNumber,
                int pageSize);

        Task<Workflow?>
            GetWorkflowByIdAsync(
                Guid workflowId);

        Task CreateWorkflowAsync(
            CreateWorkflowRequestDto dto);

        Task UpdateWorkflowAsync(
            Guid workflowId,
            UpdateWorkflowRequestDto dto);

        Task DeleteWorkflowAsync(
            Guid workflowId);
    }
}