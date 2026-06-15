using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.WorkflowInstances;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly string _connectionString;

        public WorkflowInstanceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(CreateWorkflowInstanceRequestDto dto)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_CreateWorkflowInstance", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@WorkflowId", dto.WorkflowId);
            command.Parameters.AddWithValue("@InitiatedByUserId", dto.InitiatedByUserId);
            command.Parameters.AddWithValue("@Title", dto.Title);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapWorkflowInstanceFromReader(reader);
            }

            throw new InvalidOperationException("Failed to retrieve created workflow instance.");
        }

        public async Task<WorkflowInstance?> GetWorkflowInstanceByIdAsync(Guid workflowInstanceId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_GetWorkflowInstanceById", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapWorkflowInstanceFromReader(reader);
            }

            return null;
        }

        public async Task<(IEnumerable<WorkflowInstance> Items, int TotalCount)> GetWorkflowInstancesAsync(
            string? status,
            Guid? workflowId,
            Guid? initiatedByUserId,
            int pageNumber,
            int pageSize)
        {
            List<WorkflowInstance> instances = new List<WorkflowInstance>();
            int totalCount = 0;

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_GetWorkflowInstances", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
            command.Parameters.AddWithValue("@WorkflowId", (object?)workflowId ?? DBNull.Value);
            command.Parameters.AddWithValue("@InitiatedByUserId", (object?)initiatedByUserId ?? DBNull.Value);
            command.Parameters.AddWithValue("@PageNumber", pageNumber);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (instances.Count == 0)
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                instances.Add(MapWorkflowInstanceFromReader(reader));
            }

            return (instances, totalCount);
        }

        public async Task CancelWorkflowInstanceAsync(Guid workflowInstanceId, Guid actionedByUserId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand("workflow.sp_CancelWorkflowInstance", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
            command.Parameters.AddWithValue("@ActionedByUserId", actionedByUserId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private WorkflowInstance MapWorkflowInstanceFromReader(SqlDataReader reader)
        {
            return new WorkflowInstance
            {
                WorkflowInstanceId = reader.GetGuid(reader.GetOrdinal("WorkflowInstanceId")),
                WorkflowId = reader.GetGuid(reader.GetOrdinal("WorkflowId")),
                WorkflowName = reader["WorkflowName"].ToString()!,
                InitiatedByUserId = reader.GetGuid(reader.GetOrdinal("InitiatedByUserId")),
                InitiatedByUserName = reader["InitiatedByUserName"].ToString()!,
                Title = reader["Title"].ToString()!,
                CurrentStepOrder = Convert.ToInt32(reader["CurrentStepOrder"]),
                Status = reader["Status"].ToString()!,
                CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                CompletedOn = reader["CompletedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CompletedOn"])
            };
        }
    }
}
