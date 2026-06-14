using WorkflowManagement.Application.DTOs.WorkflowSteps;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowStepService
    {
        Task<IEnumerable<WorkflowStepResponseDto>>
            GetWorkflowStepsAsync(Guid workflowId);

        Task<WorkflowStepResponseDto?>
            GetWorkflowStepByIdAsync(Guid workflowStepId);

        Task CreateWorkflowStepAsync(
            CreateWorkflowStepRequestDto dto);

        Task UpdateWorkflowStepAsync(
            Guid workflowStepId,
            UpdateWorkflowStepRequestDto dto);

        Task DeleteWorkflowStepAsync(Guid workflowStepId);
    }
}
