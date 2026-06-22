using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WorkflowManagement.Application.DTOs.Users;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)>
            GetUsersAsync(
                int pageNumber,
                int pageSize)
        {
            List<User> users = new List<User>();

            int totalCount = 0;

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetUsers",
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
                if (users.Count == 0)
                {
                    totalCount = Convert.ToInt32(
                        reader["TotalCount"]);
                }

                users.Add(new User
                {
                    UserId =
                        reader.GetGuid(
                            reader.GetOrdinal("UserId")),

                    OrganizationId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "OrganizationId")),

                    DisplayName =
                        reader["DisplayName"].ToString()!,

                    Email =
                        reader["Email"].ToString()!,

                    IsActive =
                        Convert.ToBoolean(
                            reader["IsActive"]),

                    CreatedOn =
                        Convert.ToDateTime(
                            reader["CreatedOn"]),

                    ManagerUserId =
                        reader["ManagerUserId"] == DBNull.Value
                            ? null
                            : reader.GetGuid(
                                reader.GetOrdinal("ManagerUserId"))
                });
            }

            return (users, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(
            Guid userId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetUserById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserId",
                userId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId =
                        reader.GetGuid(
                            reader.GetOrdinal("UserId")),

                    OrganizationId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "OrganizationId")),

                    DisplayName =
                        reader["DisplayName"].ToString()!,

                    Email =
                        reader["Email"].ToString()!,

                    AzureObjectId =
                        reader["AzureObjectId"] == DBNull.Value
                            ? null
                            : reader["AzureObjectId"].ToString(),

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

                    ManagerUserId =
                        reader["ManagerUserId"] == DBNull.Value
                            ? null
                            : reader.GetGuid(
                                reader.GetOrdinal("ManagerUserId")),

                    DeleteFlag =
                        Convert.ToBoolean(
                            reader["DeleteFlag"])
                };
            }

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(
            string email)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetUserByEmail",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@Email",
                email);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId =
                        reader.GetGuid(
                            reader.GetOrdinal("UserId")),

                    OrganizationId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "OrganizationId")),

                    DisplayName =
                        reader["DisplayName"].ToString()!,

                    Email =
                        reader["Email"].ToString()!,

                    AzureObjectId =
                        reader["AzureObjectId"] == DBNull.Value
                            ? null
                            : reader["AzureObjectId"].ToString(),

                    IsActive =
                        Convert.ToBoolean(
                            reader["IsActive"]),

                    CreatedOn =
                        Convert.ToDateTime(
                            reader["CreatedOn"])
                };
            }

            return null;
        }

        public async Task CreateUserAsync(
            CreateUserRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_CreateUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@OrganizationId",
                dto.OrganizationId);

            command.Parameters.AddWithValue(
                "@DisplayName",
                dto.DisplayName);

            command.Parameters.AddWithValue(
                "@Email",
                dto.Email);

            command.Parameters.AddWithValue(
                "@ManagerUserId",
                (object?)dto.ManagerUserId ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserAsync(
            Guid userId,
            UpdateUserRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_UpdateUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserId",
                userId);

            command.Parameters.AddWithValue(
                "@DisplayName",
                dto.DisplayName);

            command.Parameters.AddWithValue(
                "@Email",
                dto.Email);

            command.Parameters.AddWithValue(
                "@IsActive",
                dto.IsActive);

            command.Parameters.AddWithValue(
                "@ManagerUserId",
                (object?)dto.ManagerUserId ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_DeleteUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserId",
                userId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}
