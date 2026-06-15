using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.WorkflowTasks;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Interfaces
{
    public interface IWorkflowTaskRepository
    {
        Task<WorkflowTask?> GetTaskByIdAsync(Guid workflowTaskId);
        Task<IEnumerable<WorkflowTask>> GetMyTasksAsync(Guid userId);
        Task<IEnumerable<WorkflowTask>> GetTasksByInstanceAsync(Guid workflowInstanceId);
        Task<(string TaskStatus, string InstanceStatus)> ActionTaskAsync(ActionWorkflowTaskRequestDto dto);
    }
}
