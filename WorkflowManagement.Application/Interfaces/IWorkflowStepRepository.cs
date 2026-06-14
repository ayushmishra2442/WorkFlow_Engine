using WorkflowManagement.Application.DTOs.WorkflowSteps;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowStepRepository
    {
        Task<IEnumerable<WorkflowStep>>
            GetWorkflowStepsAsync(Guid workflowId);

        Task<WorkflowStep?> GetWorkflowStepByIdAsync(
            Guid workflowStepId);

        Task CreateWorkflowStepAsync(
            CreateWorkflowStepRequestDto dto);

        Task UpdateWorkflowStepAsync(
            Guid workflowStepId,
            UpdateWorkflowStepRequestDto dto);

        Task DeleteWorkflowStepAsync(Guid workflowStepId);
    }
}
