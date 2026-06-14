using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.Organization;
using WorkflowManagement.Domain.Entities;


namespace WorkflowManagement.Application.Interfaces
{


    public interface IOrganizationRepository
    {
        Task<Organization?>  GetOrganizationByIdAsync(Guid organizationId);




        Task<IEnumerable<Organization>>   GetOrganizationsAsync();


      


        Task  CreateOrganizationAsync(CreateOrganizationRequestDto dto);

        Task UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequestDto dto);


       Task DeleteOrganizationAsync(Guid organizationId);

    }


}
