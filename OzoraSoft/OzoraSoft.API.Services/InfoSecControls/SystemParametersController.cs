using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources;
using OzoraSoft.DataSources.InfoSecControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OzoraSoft.API.Services.InfoSecControls
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SystemParametersController : ControllerBase
    {
        private readonly OzoraSoftDBContext _context;

        public SystemParametersController(OzoraSoftDBContext context)
        {
            _context = context;
        }

        // GET: api/SystemParameters
        [HttpGet("{groupId}")]
        public async Task<ActionResult<IEnumerable<SystemParameter>>> GetSystemParameters(int groupId)
        {
            return await _context.SystemParameters.Where(sp => sp.Group_Id == groupId).ToListAsync();
        }

    }
}
