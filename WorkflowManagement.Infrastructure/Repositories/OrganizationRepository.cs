using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.Organization;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Domain.Entities;

namespace WorkflowManagement.Infrastructure.Repositories
{


    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly string _connectionString;

        public OrganizationRepository(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")!;
        }

        public async Task<Organization?>
            GetOrganizationByIdAsync(Guid organizationId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetOrganizationById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@OrganizationId",
                organizationId);

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Organization
                {
                    OrganizationId =
                        reader.GetGuid(
                            reader.GetOrdinal(
                                "OrganizationId")),

                    Name =
                        reader["Name"].ToString()!,

                    Email =
                        reader["Email"].ToString()!,

                    Phone =
                        reader["Phone"] == DBNull.Value
                            ? null
                            : reader["Phone"].ToString(),

                    Address =
                        reader["Address"] == DBNull.Value
                            ? null
                            : reader["Address"].ToString(),

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
                                reader.GetOrdinal(
                                    "CreatedBy")),

                    ModifiedOn =
                        reader["ModifiedOn"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(
                                reader["ModifiedOn"]),

                    ModifiedBy =
                        reader["ModifiedBy"] == DBNull.Value
                            ? null
                            : reader.GetGuid(
                                reader.GetOrdinal(
                                    "ModifiedBy")),

                    DeleteFlag =
                        Convert.ToBoolean(
                            reader["DeleteFlag"])
                };
            }

            return null;
        }





        public async Task<IEnumerable<Organization>>
    GetOrganizationsAsync()
        {
            List<Organization> organizations =
                new List<Organization>();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_GetOrganizations",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                organizations.Add(
                    new Organization
                    {
                        OrganizationId =
                            reader.GetGuid(
                                reader.GetOrdinal(
                                    "OrganizationId")),

                        Name =
                            reader["Name"].ToString()!,

                        Email =
                            reader["Email"].ToString()!,

                        Phone =
                            reader["Phone"] == DBNull.Value
                                ? null
                                : reader["Phone"].ToString(),

                        Address =
                            reader["Address"] == DBNull.Value
                                ? null
                                : reader["Address"].ToString(),

                        IsActive =
                            Convert.ToBoolean(
                                reader["IsActive"]),

                        CreatedOn =
                            Convert.ToDateTime(
                                reader["CreatedOn"]),

                        //CreatedBy =
                        //    reader["CreatedBy"] == DBNull.Value
                        //        ? null
                        //        : reader.GetGuid(
                        //            reader.GetOrdinal(
                        //                "CreatedBy")),

                        //ModifiedOn =
                        //    reader["ModifiedOn"] == DBNull.Value
                        //        ? null
                        //        : Convert.ToDateTime(
                        //            reader["ModifiedOn"]),

                        //ModifiedBy =
                        //    reader["ModifiedBy"] == DBNull.Value
                        //        ? null
                        //        : reader.GetGuid(
                        //            reader.GetOrdinal(
                        //                "ModifiedBy")),

                        //DeleteFlag =
                        //    Convert.ToBoolean(
                        //        reader["DeleteFlag"])
                    });
            }

            return organizations;
        }






        public async Task CreateOrganizationAsync(
            CreateOrganizationRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_CreateOrganization",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@Name",
                dto.Name);

            command.Parameters.AddWithValue(
                "@Email",
                dto.Email);

            command.Parameters.AddWithValue(
                "@Phone",
                dto.Phone ?? (object)DBNull.Value);

            command.Parameters.AddWithValue(
                "@Address",
                dto.Address ?? (object)DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }





        public async Task UpdateOrganizationAsync(
    Guid organizationId,
    UpdateOrganizationRequestDto dto)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_UpdateOrganization",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@OrganizationId",
                organizationId);

            command.Parameters.AddWithValue(
                "@Name",
                dto.Name);

            command.Parameters.AddWithValue(
                "@Email",
                dto.Email);

            command.Parameters.AddWithValue(
                "@Phone",
                dto.Phone ?? (object)DBNull.Value);

            command.Parameters.AddWithValue(
                "@Address",
                dto.Address ?? (object)DBNull.Value);

            command.Parameters.AddWithValue(
                "@IsActive",
                dto.IsActive);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }


        public async Task DeleteOrganizationAsync(
    Guid organizationId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            using SqlCommand command =
                new SqlCommand(
                    "auth.sp_DeleteOrganization",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@OrganizationId",
                organizationId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }




    }



}
