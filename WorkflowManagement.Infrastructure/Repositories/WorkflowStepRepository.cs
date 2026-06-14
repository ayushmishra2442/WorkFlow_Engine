using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WorkflowManagement.Application.DTOs.WorkflowSteps;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class WorkflowStepRepository
        : IWorkflowStepRepository
    {
        private readonly string _connectionString;

        public WorkflowStepRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task<IEnumerable<WorkflowStep>>
            GetWorkflowStepsAsync(Guid workflowId)
        {
            List<WorkflowStep> steps =
                new List<WorkflowStep>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_GetWorkflowSteps",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowId",
                workflowId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                steps.Add(new WorkflowStep
                {
                    WorkflowStepId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "WorkflowStepId")),

                    WorkflowId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "WorkflowId")),

                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

                    RoleName =
                        reader["RoleName"].ToString()!,

                    StepName =
                        reader["StepName"].ToString()!,

                    StepOrder =
                        Convert.ToInt32(
                            reader["StepOrder"]),

                    Description =
                        reader["Description"] == DBNull.Value
                            ? null
                            : reader["Description"].ToString(),

                    IsActive =
                        Convert.ToBoolean(
                            reader["IsActive"]),

                    CreatedOn =
                        Convert.ToDateTime(
                            reader["CreatedOn"])
                });
            }

            return steps;
        }

        public async Task<WorkflowStep?>
            GetWorkflowStepByIdAsync(Guid workflowStepId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_GetWorkflowStepById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowStepId",
                workflowStepId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new WorkflowStep
                {
                    WorkflowStepId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "WorkflowStepId")),

                    WorkflowId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "WorkflowId")),

                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

                    RoleName =
                        reader["RoleName"].ToString()!,

                    StepName =
                        reader["StepName"].ToString()!,

                    StepOrder =
                        Convert.ToInt32(
                            reader["StepOrder"]),

                    Description =
                        reader["Description"] == DBNull.Value
                            ? null
                            : reader["Description"].ToString(),

                    IsActive =
                        Convert.ToBoolean(
                            reader["IsActive"]),

                    CreatedOn =
                        Convert.ToDateTime(
                            reader["CreatedOn"]),

                    CreatedBy =
                        reader["CreatedBy"] == DBNull.Value
                            ? null
                            : reader.GetGuid(
                                reader.GetOrdinal("CreatedBy")),

                    ModifiedOn =
                        reader["ModifiedOn"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(
                                reader["ModifiedOn"]),

                    ModifiedBy =
                        reader["ModifiedBy"] == DBNull.Value
                            ? null
                            : reader.GetGuid(
                                reader.GetOrdinal("ModifiedBy")),

                    DeleteFlag =
                        Convert.ToBoolean(
                            reader["DeleteFlag"])
                };
            }

            return null;
        }

        public async Task CreateWorkflowStepAsync(
            CreateWorkflowStepRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_CreateWorkflowStep",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowId",
                dto.WorkflowId);

            command.Parameters.AddWithValue(
                "@RoleId",
                dto.RoleId);

            command.Parameters.AddWithValue(
                "@StepName",
                dto.StepName);

            command.Parameters.AddWithValue(
                "@StepOrder",
                dto.StepOrder);

            command.Parameters.AddWithValue(
                "@Description",
                (object?)dto.Description
                ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateWorkflowStepAsync(
            Guid workflowStepId,
            UpdateWorkflowStepRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_UpdateWorkflowStep",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowStepId",
                workflowStepId);

            command.Parameters.AddWithValue(
                "@RoleId",
                dto.RoleId);

            command.Parameters.AddWithValue(
                "@StepName",
                dto.StepName);

            command.Parameters.AddWithValue(
                "@StepOrder",
                dto.StepOrder);

            command.Parameters.AddWithValue(
                "@Description",
                (object?)dto.Description
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@IsActive",
                dto.IsActive);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteWorkflowStepAsync(
            Guid workflowStepId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_DeleteWorkflowStep",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowStepId",
                workflowStepId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}
