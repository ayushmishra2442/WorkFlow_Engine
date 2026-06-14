using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Application.DTOs.Organization;




namespace WorkflowManagement.Application.Interfaces
{

    public interface IOrganizationService
    {
        Task<OrganizationResponseDto?>  GetOrganizationByIdAsync(Guid organizationId);
   
    
    

        Task<IEnumerable<OrganizationResponseDto>> GetOrganizationsAsync();





        Task CreateOrganizationAsync(CreateOrganizationRequestDto dto);



      Task UpdateOrganizationAsync( Guid organizationId, UpdateOrganizationRequestDto dto);



      Task DeleteOrganizationAsync(Guid organizationId);


    }


    


}
