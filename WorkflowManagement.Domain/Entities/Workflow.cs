using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowManagement.Domain.Entities
{

    public class Workflow
    {
        public Guid WorkflowId { get; set; }

        public Guid OrganizationId { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool DeleteFlag { get; set; }
    }



}
