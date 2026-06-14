using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WorkflowManagement.Application.DTOs.Workflows;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly string _connectionString;

        public WorkflowRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task CreateWorkflowAsync(
            CreateWorkflowRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_CreateWorkflow",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@OrganizationId",
                dto.OrganizationId);

            command.Parameters.AddWithValue(
                "@Name",
                dto.Name);

            command.Parameters.AddWithValue(
                "@Description",
                (object?)dto.Description
                ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

     

        public async Task<(IEnumerable<Workflow> Items, int TotalCount)>
    GetWorkflowsAsync(
        int pageNumber,
        int pageSize)
        {
            List<Workflow> workflows =
                new List<Workflow>();

            int totalCount = 0;

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_GetWorkflows",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@PageNumber",
                pageNumber);

            command.Parameters.AddWithValue(
                "@PageSize",
                pageSize);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Read TotalCount from first row
                if (workflows.Count == 0)
                {
                    totalCount = Convert.ToInt32(
                        reader["TotalCount"]);
                }

                workflows.Add(
                    new Workflow
                    {
                        WorkflowId =
                            reader.GetGuid(
                                reader.GetOrdinal(
                                    "WorkflowId")),

                        OrganizationId =
                            reader.GetGuid(
                                reader.GetOrdinal(
                                    "OrganizationId")),

                        Name =
                            reader["Name"].ToString()!,

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

                        DeleteFlag =
                            Convert.ToBoolean(
                                reader["DeleteFlag"])
                    });
            }

            return (workflows, totalCount);
        }










        public async Task<Workflow?>
    GetWorkflowByIdAsync(
        Guid workflowId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_GetWorkflowById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowId",
                workflowId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Workflow
                {
                    WorkflowId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "WorkflowId")),

                    OrganizationId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "OrganizationId")),

                    Name =
                        reader["Name"].ToString()!,

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

                    DeleteFlag =
                        Convert.ToBoolean(
                            reader["DeleteFlag"])
                };
            }

            return null;
        }




        public async Task UpdateWorkflowAsync(
            Guid workflowId,
            UpdateWorkflowRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_UpdateWorkflow",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowId",
                workflowId);

            command.Parameters.AddWithValue(
                "@Name",
                dto.Name);

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

        public async Task DeleteWorkflowAsync(
            Guid workflowId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "workflow.sp_DeleteWorkflow",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@WorkflowId",
                workflowId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}