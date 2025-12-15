using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzoraSoft.API.Services.Models;
using OzoraSoft.DataSources;
using OzoraSoft.DataSources.Shared;
using OzoraSoft.Library.Enums.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OzoraSoft.API.Services.Shared
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventLogsController : ControllerBase
    {
        private readonly OzoraSoft_Shared_DBContext _context;

        public EventLogsController(OzoraSoft_Shared_DBContext context)
        {
            _context = context;
        }

        // GET: api/EventLogs/id
        [HttpGet("{id}")]
        public async Task<ActionResult<EventLog>> GetOne(int id)
        {
            var result = await _context.EventLogs.Where(x => x.id == id).FirstOrDefaultAsync();
            return result!;
        }

        // GET: api/EventLogs/
        [HttpPost("list")]
        public async Task<ActionResult<IEnumerable<EventLog>>> PostList([FromBody] EventLog_Filter filter)
        {
            var result = new List<EventLog>();
            var query = _context.EventLogs.AsQueryable();
            DateTime dt_process_date = Convert.ToDateTime(filter.process_datetime);

            result = await query.Where(el =>
                (filter.project_id == 0 || el.project_id == filter.project_id) &&
                (filter.module_id == 0 || el.module_id == filter.module_id) &&
                (filter.controller_id == 0 || el.controller_id == filter.controller_id) &&
                (filter.action_id == 0 || el.action_id == filter.action_id) &&
                (string.IsNullOrEmpty(filter.process_datetime) || el.process_datetime.Date  == dt_process_date.Date) &&
                (string.IsNullOrEmpty(filter.user_name) || el.user_name.Contains(filter.user_name))
            ).ToListAsync();

            return result;
        }


        // POST: api/EventLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<int>> PostRecord([FromBody] EventLog eventLog)
        {
            _context.EventLogs.Add(eventLog);
            await _context.SaveChangesAsync();

            return eventLog.id;
        }


        private bool EventLogExists(int id)
        {
            return _context.EventLogs.Any(e => e.id == id);
        }
    }
}
