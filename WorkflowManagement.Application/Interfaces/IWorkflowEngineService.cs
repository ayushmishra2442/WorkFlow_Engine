using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.WorkflowInstances;
using WorkflowManagement.Application.DTOs.WorkflowTasks;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowInstanceResponseDto> StartWorkflowInstanceAsync(CreateWorkflowInstanceRequestDto dto);
        Task<WorkflowInstanceResponseDto?> GetWorkflowInstanceByIdAsync(Guid workflowInstanceId);
        Task<PagedResponse<WorkflowInstanceResponseDto>> GetWorkflowInstancesAsync(
            string? status,
            Guid? workflowId,
            Guid? initiatedByUserId,
            int pageNumber,
            int pageSize);
        Task<IEnumerable<WorkflowTaskResponseDto>> GetMyTasksAsync(Guid userId);
        Task<IEnumerable<WorkflowTaskResponseDto>> GetTasksByInstanceAsync(Guid workflowInstanceId);
        Task ActionTaskAsync(ActionWorkflowTaskRequestDto dto);
        Task CancelWorkflowInstanceAsync(Guid workflowInstanceId, Guid actionedByUserId);
    }
}
