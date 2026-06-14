using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using WorkflowManagement.Application.DTOs.UserRoles;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly string _connectionString;

        public UserRoleRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task<IEnumerable<UserRoleResponseDto>>
            GetRolesForUserAsync(Guid userId)
        {
            List<UserRoleResponseDto> results =
                new List<UserRoleResponseDto>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetRolesForUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserId",
                userId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new UserRoleResponseDto
                {
                    UserRoleId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "UserRoleId")),

                    UserId =
                        reader.GetGuid(
                            reader.GetOrdinal("UserId")),

                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

                    RoleName =
                        reader["RoleName"].ToString()!,

                    AssignedOn =
                        Convert.ToDateTime(
                            reader["AssignedOn"])
                });
            }

            return results;
        }

        public async Task<IEnumerable<UserRoleResponseDto>>
            GetUsersInRoleAsync(Guid roleId)
        {
            List<UserRoleResponseDto> results =
                new List<UserRoleResponseDto>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetUsersInRole",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@RoleId",
                roleId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new UserRoleResponseDto
                {
                    UserRoleId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "UserRoleId")),

                    UserId =
                        reader.GetGuid(
                            reader.GetOrdinal("UserId")),

                    RoleId =
                        reader.GetGuid(
                            reader.GetOrdinal("RoleId")),

                    RoleName =
                        reader["RoleName"].ToString()!,

                    AssignedOn =
                        Convert.ToDateTime(
                            reader["AssignedOn"])
                });
            }

            return results;
        }

        public async Task AssignRoleToUserAsync(
            AssignRoleRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_AssignRoleToUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserId",
                dto.UserId);

            command.Parameters.AddWithValue(
                "@RoleId",
                dto.RoleId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task RemoveRoleFromUserAsync(
            Guid userRoleId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_RemoveRoleFromUser",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@UserRoleId",
                userRoleId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }
    }
}
