using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Application.DTOs.Organization;

namespace WorkflowManagement.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationService(
            IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task<OrganizationResponseDto?>
            GetOrganizationByIdAsync(Guid organizationId)
        {
            var organization =
                await _organizationRepository
                    .GetOrganizationByIdAsync(organizationId);

            if (organization == null)
            {
                return null;
            }

            return new OrganizationResponseDto
            {
                OrganizationId = organization.OrganizationId,
                Name = organization.Name,
                Email = organization.Email,
                Phone = organization.Phone,
                Address = organization.Address,
                IsActive = organization.IsActive,
                CreatedOn = organization.CreatedOn
            };
        }

        public async Task<IEnumerable<OrganizationResponseDto>>
            GetOrganizationsAsync()
        {

           


            var organizations =
                await _organizationRepository
                    .GetOrganizationsAsync();

            return organizations.Select(
                organization => new OrganizationResponseDto
                {
                    OrganizationId =
                        organization.OrganizationId,

                    Name =
                        organization.Name,

                    Email =
                        organization.Email,

                    Phone =
                        organization.Phone,

                    Address =
                        organization.Address,

                    IsActive =
                        organization.IsActive,

                    CreatedOn =
                        organization.CreatedOn
                });
        }

        public async Task CreateOrganizationAsync(
            CreateOrganizationRequestDto dto)
        {
            await _organizationRepository
                .CreateOrganizationAsync(dto);
        }



        public async Task UpdateOrganizationAsync( Guid organizationId, UpdateOrganizationRequestDto dto)
       
        
        {
           
            
            await _organizationRepository.UpdateOrganizationAsync( organizationId, dto);
        
        
        
        }


        public async Task DeleteOrganizationAsync(
    Guid organizationId)
        {
            await _organizationRepository
                .DeleteOrganizationAsync(
                    organizationId);
        }



    }
}