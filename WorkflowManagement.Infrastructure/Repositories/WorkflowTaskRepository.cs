using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.WorkflowTasks;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class WorkflowTaskRepository : IWorkflowTaskRepository
    {
        private readonly string _connectionString;

        public WorkflowTaskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<WorkflowTask?> GetTaskByIdAsync(Guid workflowTaskId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(
                @"SELECT 
                    wt.[WorkflowTaskId],
                    wt.[WorkflowInstanceId],
                    wi.[Title] AS [WorkflowInstanceTitle],
                    wi.[WorkflowId],
                    w.[Name] AS [WorkflowName],
                    wi.[InitiatedByUserId],
                    iu.[DisplayName] AS [InitiatedByUserName],
                    wt.[WorkflowStepId],
                    ws.[StepName],
                    ws.[StepOrder],
                    wt.[AssignedToRoleId],
                    r.[Name] AS [AssignedToRoleName],
                    wt.[AssignedToUserId],
                    au.[DisplayName] AS [AssignedToUserName],
                    wt.[Status],
                    wt.[Comments],
                    wt.[AssignedOn],
                    wt.[ActionedOn],
                    wt.[ActionedByUserId],
                    actu.[DisplayName] AS [ActionedByUserName]
                  FROM [workflow].[WorkflowTasks] wt
                  INNER JOIN [workflow].[WorkflowInstances] wi ON wt.[WorkflowInstanceId] = wi.[WorkflowInstanceId]
                  INNER JOIN [workflow].[Workflows] w ON wi.[WorkflowId] = w.[WorkflowId]
                  INNER JOIN [workflow].[WorkflowSteps] ws ON wt.[WorkflowStepId] = ws.[WorkflowStepId]
                  INNER JOIN [auth].[Roles] r ON wt.[AssignedToRoleId] = r.[RoleId]
                  INNER JOIN [auth].[Users] iu ON wi.[InitiatedByUserId] = iu.[UserId]
                  LEFT JOIN [auth].[Users] au ON wt.[AssignedToUserId] = au.[UserId]
                  LEFT JOIN [auth].[Users] actu ON wt.[ActionedByUserId] = actu.[UserId]
                  WHERE wt.[WorkflowTaskId] = @WorkflowTaskId", connection);

            command.Parameters.AddWithValue("@WorkflowTaskId", workflowTaskId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapWorkflowTaskFromReader(reader);
            }

            return null;
        }

        public async Task<IEnumerable<WorkflowTask>> GetMyTasksAsync(Guid userId)
        {
            List<WorkflowTask> tasks = new List<WorkflowTask>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_GetMyTasks", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(MapWorkflowTaskFromReader(reader, includeAuditData: false));
            }

            return tasks;
        }

        public async Task<IEnumerable<WorkflowTask>> GetTasksByInstanceAsync(Guid workflowInstanceId)
        {
            List<WorkflowTask> tasks = new List<WorkflowTask>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_GetTasksByInstance", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(MapWorkflowTaskFromReader(reader, includeAuditData: true));
            }

            return tasks;
        }

        public async Task<(string TaskStatus, string InstanceStatus)> ActionTaskAsync(ActionWorkflowTaskRequestDto dto)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_ActionTask", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@WorkflowTaskId", dto.WorkflowTaskId);
            command.Parameters.AddWithValue("@ActionedByUserId", dto.ActionedByUserId);
            command.Parameters.AddWithValue("@Status", dto.Status);
            command.Parameters.AddWithValue("@Comments", (object?)dto.Comments ?? DBNull.Value);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string taskStatus = reader["TaskStatus"].ToString()!;
                string instanceStatus = reader["InstanceStatus"].ToString()!;
                return (taskStatus, instanceStatus);
            }

            throw new InvalidOperationException("Failed to retrieve task action result.");
        }

        private WorkflowTask MapWorkflowTaskFromReader(SqlDataReader reader, bool includeAuditData = true)
        {
            var task = new WorkflowTask
            {
                WorkflowTaskId = reader.GetGuid(reader.GetOrdinal("WorkflowTaskId")),
                WorkflowInstanceId = reader.GetGuid(reader.GetOrdinal("WorkflowInstanceId")),
                WorkflowStepId = reader.GetGuid(reader.GetOrdinal("WorkflowStepId")),
                StepName = reader["StepName"].ToString()!,
                StepOrder = Convert.ToInt32(reader["StepOrder"]),
                AssignedToRoleId = reader.GetGuid(reader.GetOrdinal("AssignedToRoleId")),
                AssignedToRoleName = reader["AssignedToRoleName"].ToString()!,
                Status = reader["Status"].ToString()!,
                AssignedOn = Convert.ToDateTime(reader["AssignedOn"])
            };

            // Conditionally read properties that might not be in simple list SPs or might be null
            if (includeAuditData)
            {
                task.WorkflowInstanceTitle = reader.HasColumn("WorkflowInstanceTitle") ? reader["WorkflowInstanceTitle"].ToString()! : string.Empty;
                task.WorkflowId = reader.HasColumn("WorkflowId") ? reader.GetGuid(reader.GetOrdinal("WorkflowId")) : Guid.Empty;
                task.WorkflowName = reader.HasColumn("WorkflowName") ? reader["WorkflowName"].ToString()! : string.Empty;
                task.InitiatedByUserId = reader.HasColumn("InitiatedByUserId") ? reader.GetGuid(reader.GetOrdinal("InitiatedByUserId")) : Guid.Empty;
                task.InitiatedByUserName = reader.HasColumn("InitiatedByUserName") ? reader["InitiatedByUserName"].ToString()! : string.Empty;

                task.AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? null : reader.GetGuid(reader.GetOrdinal("AssignedToUserId"));
                task.AssignedToUserName = reader.HasColumn("AssignedToUserName") && !reader.IsDBNull(reader.GetOrdinal("AssignedToUserName")) ? reader["AssignedToUserName"].ToString()! : string.Empty;
                
                task.Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader["Comments"].ToString();
                task.ActionedOn = reader.IsDBNull(reader.GetOrdinal("ActionedOn")) ? null : Convert.ToDateTime(reader["ActionedOn"]);
                task.ActionedByUserId = reader.IsDBNull(reader.GetOrdinal("ActionedByUserId")) ? null : reader.GetGuid(reader.GetOrdinal("ActionedByUserId"));
                task.ActionedByUserName = reader.HasColumn("ActionedByUserName") && !reader.IsDBNull(reader.GetOrdinal("ActionedByUserName")) ? reader["ActionedByUserName"].ToString()! : string.Empty;
            }
            else
            {
                // Simple mappings for sp_GetMyTasks
                task.WorkflowInstanceTitle = reader["WorkflowInstanceTitle"].ToString()!;
                task.WorkflowId = reader.GetGuid(reader.GetOrdinal("WorkflowId"));
                task.WorkflowName = reader["WorkflowName"].ToString()!;
                task.InitiatedByUserId = reader.GetGuid(reader.GetOrdinal("InitiatedByUserId"));
                task.InitiatedByUserName = reader["InitiatedByUserName"].ToString()!;
                task.AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? null : reader.GetGuid(reader.GetOrdinal("AssignedToUserId"));
            }

            return task;
        }
    }

    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
