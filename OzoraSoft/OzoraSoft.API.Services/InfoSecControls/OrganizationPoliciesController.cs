using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources;
using OzoraSoft.DataSources.InfoSecControls;

namespace OzoraSoft.API.Services.InfoSecControls
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationPoliciesController : ControllerBase
    {
        private readonly OzoraSoft_InfoSecControls_DBContext _context;

        public OrganizationPoliciesController(OzoraSoft_InfoSecControls_DBContext context)
        {
            _context = context;
        }

        // GET: api/OrganizationPolicies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationPolicy>>> GetList()
        {
            return await _context.OrganizationPolicies.ToListAsync();
        }

        // GET: api/OrganizationPolicies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationPolicy>> GetOne(int id)
        {
            var organizationPolicy = await _context.OrganizationPolicies.FindAsync(id);

            if (organizationPolicy == null)
            {
                return NotFound();
            }

            return organizationPolicy;
        }
    }
}
