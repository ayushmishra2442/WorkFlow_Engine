using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.WorkflowInstances;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowInstanceRepository
    {
        Task<WorkflowInstance> CreateWorkflowInstanceAsync(CreateWorkflowInstanceRequestDto dto);
        Task<WorkflowInstance?> GetWorkflowInstanceByIdAsync(Guid workflowInstanceId);
        Task<(IEnumerable<WorkflowInstance> Items, int TotalCount)> GetWorkflowInstancesAsync(
            string? status,
            Guid? workflowId,
            Guid? initiatedByUserId,
            int pageNumber,
            int pageSize);
        Task CancelWorkflowInstanceAsync(Guid workflowInstanceId, Guid actionedByUserId);
    }
}
