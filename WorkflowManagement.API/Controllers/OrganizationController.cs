using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Application.DTOs.Organization;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : BaseApiController
    {


        private readonly IOrganizationService _organizationService;

        public OrganizationController(
            IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        
        
        
        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetOrganizationById(Guid id)
        {
            var organization =
                await _organizationService
                    .GetOrganizationByIdAsync(id);

            if (organization == null)
            {


                return ApiNotFound(
           "Organization not found");


            }



            return ApiOk(organization);


        }





        [HttpGet("GetOrganization")]
        public async Task<IActionResult>
    GetOrganizations()
        {
            var organizations =
                await _organizationService.GetOrganizationsAsync();

            return ApiOk(organizations);
        }





        [HttpPost("CreateOrganization")]
        public async Task<IActionResult>
    CreateOrganization(
        [FromBody]
        CreateOrganizationRequestDto dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            await _organizationService
                .CreateOrganizationAsync(dto);


            return ApiOk(
    "Organization created successfully");

        }



        [HttpPut("{id}")]
        public async Task<IActionResult>
    UpdateOrganization(
        Guid id,
        [FromBody]
        UpdateOrganizationRequestDto dto)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }




            await _organizationService
                .UpdateOrganizationAsync(
                    id,
                    dto);


            return ApiOk(
    "Organization updated successfully");


        }





        [HttpDelete("{id}")]
        public async Task<IActionResult>
    DeleteOrganization(Guid id)
        {
            await _organizationService
                .DeleteOrganizationAsync(id);



            return ApiOk(
    "Organization deleted successfully");


        }




    }
}
 