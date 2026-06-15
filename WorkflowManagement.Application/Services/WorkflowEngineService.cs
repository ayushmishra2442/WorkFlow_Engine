using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowManagement.Application.Common;
using WorkflowManagement.Application.DTOs.WorkflowInstances;
using WorkflowManagement.Application.DTOs.WorkflowTasks;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Application.Services
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly IWorkflowInstanceRepository _instanceRepository;
        private readonly IWorkflowTaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ILogger<WorkflowEngineService> _logger;

        public WorkflowEngineService(
            IWorkflowInstanceRepository instanceRepository,
            IWorkflowTaskRepository taskRepository,
            IUserRepository userRepository,
            IWorkflowRepository workflowRepository,
            IUserRoleRepository userRoleRepository,
            ILogger<WorkflowEngineService> logger)
        {
            _instanceRepository = instanceRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _workflowRepository = workflowRepository;
            _userRoleRepository = userRoleRepository;
            _logger = logger;
        }

        public async Task<WorkflowInstanceResponseDto> StartWorkflowInstanceAsync(CreateWorkflowInstanceRequestDto dto)
        {
            _logger.LogInformation("Starting workflow instance '{Title}' for Workflow {WorkflowId}", dto.Title, dto.WorkflowId);

            // 1. Verify user exists
            var user = await _userRepository.GetUserByIdAsync(dto.InitiatedByUserId);
            if (user == null)
            {
                _logger.LogWarning("StartWorkflowInstance failed — User {UserId} not found", dto.InitiatedByUserId);
                throw new KeyNotFoundException($"User with ID '{dto.InitiatedByUserId}' was not found.");
            }

            // 2. Verify workflow exists
            var workflow = await _workflowRepository.GetWorkflowByIdAsync(dto.WorkflowId);
            if (workflow == null)
            {
                _logger.LogWarning("StartWorkflowInstance failed — Workflow {WorkflowId} not found", dto.WorkflowId);
                throw new KeyNotFoundException($"Workflow with ID '{dto.WorkflowId}' was not found.");
            }

            if (!workflow.IsActive)
            {
                _logger.LogWarning("StartWorkflowInstance failed — Workflow {WorkflowId} is inactive", dto.WorkflowId);
                throw new ArgumentException("Cannot start an inactive workflow.");
            }

            // 3. Execute database operation
            var instance = await _instanceRepository.CreateWorkflowInstanceAsync(dto);

            _logger.LogInformation("Workflow instance '{Title}' started successfully with ID {InstanceId}", dto.Title, instance.WorkflowInstanceId);
            
            return MapInstanceToDto(instance);
        }

        public async Task<WorkflowInstanceResponseDto?> GetWorkflowInstanceByIdAsync(Guid workflowInstanceId)
        {
            var instance = await _instanceRepository.GetWorkflowInstanceByIdAsync(workflowInstanceId);
            if (instance == null)
            {
                return null;
            }

            return MapInstanceToDto(instance);
        }

        public async Task<PagedResponse<WorkflowInstanceResponseDto>> GetWorkflowInstancesAsync(
            string? status,
            Guid? workflowId,
            Guid? initiatedByUserId,
            int pageNumber,
            int pageSize)
        {
            var (items, totalCount) = await _instanceRepository.GetWorkflowInstancesAsync(
                status,
                workflowId,
                initiatedByUserId,
                pageNumber,
                pageSize);

            var dtos = items.Select(MapInstanceToDto);

            return new PagedResponse<WorkflowInstanceResponseDto>(
                dtos,
                pageNumber,
                pageSize,
                totalCount);
        }

        public async Task<IEnumerable<WorkflowTaskResponseDto>> GetMyTasksAsync(Guid userId)
        {
            // Verify user exists first
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{userId}' was not found.");
            }

            var tasks = await _taskRepository.GetMyTasksAsync(userId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<WorkflowTaskResponseDto>> GetTasksByInstanceAsync(Guid workflowInstanceId)
        {
            // Verify instance exists
            var instance = await _instanceRepository.GetWorkflowInstanceByIdAsync(workflowInstanceId);
            if (instance == null)
            {
                throw new KeyNotFoundException($"Workflow instance with ID '{workflowInstanceId}' was not found.");
            }

            var tasks = await _taskRepository.GetTasksByInstanceAsync(workflowInstanceId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task ActionTaskAsync(ActionWorkflowTaskRequestDto dto)
        {
            _logger.LogInformation("User {UserId} is actioning task {TaskId} with status {Status}", dto.ActionedByUserId, dto.WorkflowTaskId, dto.Status);

            // 1. Verify user exists
            var user = await _userRepository.GetUserByIdAsync(dto.ActionedByUserId);
            if (user == null)
            {
                _logger.LogWarning("ActionTask failed — User {UserId} not found", dto.ActionedByUserId);
                throw new KeyNotFoundException($"User with ID '{dto.ActionedByUserId}' was not found.");
            }

            // 2. Verify task exists
            var task = await _taskRepository.GetTaskByIdAsync(dto.WorkflowTaskId);
            if (task == null)
            {
                _logger.LogWarning("ActionTask failed — Task {TaskId} not found", dto.WorkflowTaskId);
                throw new KeyNotFoundException($"Task with ID '{dto.WorkflowTaskId}' was not found.");
            }

            if (task.Status != "Pending")
            {
                _logger.LogWarning("ActionTask failed — Task {TaskId} is already in status '{Status}'", dto.WorkflowTaskId, task.Status);
                throw new ArgumentException($"Task is already in status '{task.Status}' and cannot be actioned.");
            }

            // 3. Verify role assignment (Security Check)
            if (task.AssignedToUserId.HasValue)
            {
                if (task.AssignedToUserId.Value != dto.ActionedByUserId)
                {
                    _logger.LogWarning("ActionTask failed — Task {TaskId} is explicitly assigned to User {AssignedUserId}, but User {ActionedUserId} tried to action it", 
                        dto.WorkflowTaskId, task.AssignedToUserId, dto.ActionedByUserId);
                    throw new UnauthorizedAccessException("You are not authorized to action this task as it is assigned to another user.");
                }
            }
            else
            {
                var userRoles = await _userRoleRepository.GetRolesForUserAsync(dto.ActionedByUserId);
                bool hasAssignedRole = userRoles.Any(ur => ur.RoleId == task.AssignedToRoleId);

                if (!hasAssignedRole)
                {
                    _logger.LogWarning("ActionTask failed — User {UserId} does not have required Role {RoleId} ({RoleName}) to action Task {TaskId}", 
                        dto.ActionedByUserId, task.AssignedToRoleId, task.AssignedToRoleName, dto.WorkflowTaskId);
                    throw new UnauthorizedAccessException($"You do not have the required role '{task.AssignedToRoleName}' to action this task.");
                }
            }

            // 4. Perform database action
            var (taskStatus, instanceStatus) = await _taskRepository.ActionTaskAsync(dto);

            _logger.LogInformation("Task {TaskId} actioned successfully. Task Status: {TaskStatus}, Instance Status: {InstanceStatus}", 
                dto.WorkflowTaskId, taskStatus, instanceStatus);
        }

        public async Task CancelWorkflowInstanceAsync(Guid workflowInstanceId, Guid actionedByUserId)
        {
            _logger.LogInformation("User {UserId} is cancelling workflow instance {InstanceId}", actionedByUserId, workflowInstanceId);

            // 1. Verify user exists
            var user = await _userRepository.GetUserByIdAsync(actionedByUserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{actionedByUserId}' was not found.");
            }

            // 2. Verify instance exists
            var instance = await _instanceRepository.GetWorkflowInstanceByIdAsync(workflowInstanceId);
            if (instance == null)
            {
                throw new KeyNotFoundException($"Workflow instance with ID '{workflowInstanceId}' was not found.");
            }

            // 3. Execute cancellation
            await _instanceRepository.CancelWorkflowInstanceAsync(workflowInstanceId, actionedByUserId);

            _logger.LogInformation("Workflow instance {InstanceId} successfully cancelled by User {UserId}", workflowInstanceId, actionedByUserId);
        }

        private WorkflowInstanceResponseDto MapInstanceToDto(WorkflowInstance entity)
        {
            return new WorkflowInstanceResponseDto
            {
                WorkflowInstanceId = entity.WorkflowInstanceId,
                WorkflowId = entity.WorkflowId,
                WorkflowName = entity.WorkflowName,
                InitiatedByUserId = entity.InitiatedByUserId,
                InitiatedByUserName = entity.InitiatedByUserName,
                Title = entity.Title,
                CurrentStepOrder = entity.CurrentStepOrder,
                Status = entity.Status,
                CreatedOn = entity.CreatedOn,
                CompletedOn = entity.CompletedOn
            };
        }

        private WorkflowTaskResponseDto MapTaskToDto(WorkflowTask entity)
        {
            return new WorkflowTaskResponseDto
            {
                WorkflowTaskId = entity.WorkflowTaskId,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                WorkflowInstanceTitle = entity.WorkflowInstanceTitle,
                WorkflowId = entity.WorkflowId,
                WorkflowName = entity.WorkflowName,
                InitiatedByUserId = entity.InitiatedByUserId,
                InitiatedByUserName = entity.InitiatedByUserName,
                WorkflowStepId = entity.WorkflowStepId,
                StepName = entity.StepName,
                StepOrder = entity.StepOrder,
                AssignedToRoleId = entity.AssignedToRoleId,
                AssignedToRoleName = entity.AssignedToRoleName,
                AssignedToUserId = entity.AssignedToUserId,
                AssignedToUserName = entity.AssignedToUserName,
                Status = entity.Status,
                Comments = entity.Comments,
                AssignedOn = entity.AssignedOn,
                ActionedOn = entity.ActionedOn,
                ActionedByUserId = entity.ActionedByUserId,
                ActionedByUserName = entity.ActionedByUserName
            };
        }
    }
}
