using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WorkflowManagement.Application.DTOs.Roles;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task<IEnumerable<Role>>
            GetRolesAsync()
        {
            List<Role> roles = new List<Role>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetRoles",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(new Role
                {
                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

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
                            reader["CreatedOn"])
                });
            }

            return roles;
        }

        public async Task<Role?> GetRoleByIdAsync(
            Guid roleId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetRoleById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@RoleId",
                roleId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Role
                {
                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

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

        public async Task CreateRoleAsync(
            CreateRoleRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_CreateRole",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

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

        public async Task UpdateRoleAsync(
            Guid roleId,
            UpdateRoleRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_UpdateRole",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@RoleId",
                roleId);

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

        public async Task DeleteRoleAsync(Guid roleId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_DeleteRole",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@RoleId",
                roleId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}
