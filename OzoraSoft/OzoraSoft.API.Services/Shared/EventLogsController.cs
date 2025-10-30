using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzoraSoft.API.Services.Models;
using OzoraSoft.DataSources;
using OzoraSoft.DataSources.Shared;
using OzoraSoft.Library.Enums.Shared;

namespace OzoraSoft.API.Services.Shared
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventLogsController : ControllerBase
    {
        private readonly OzoraSoft_Shared_DBContext _context;

        public EventLogsController(OzoraSoft_Shared_DBContext context)
        {
            _context = context;
        }

        // GET: api/EventLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventLog>>> GetEventLogs()
        {
            return await _context.EventLogs.ToListAsync();
        }

        // GET: api/EventLogs/5
        [HttpPost("list")]
        public async Task<ActionResult<IEnumerable<EventLog>>> PostEventLogList([FromBody] EventLog_Filter filter)
        {
            var result = new List<EventLog>();
            var query = _context.EventLogs.AsQueryable();
            DateTime dt_process_date = Convert.ToDateTime(filter.process_datetime);

            result = await query.Where(el =>
                (filter.project_id == 0 || el.project_id == filter.project_id) &&
                (filter.module_id == 0 || el.module_id == filter.module_id) &&
                (filter.entity_id == 0 || el.entity_id == filter.entity_id) &&
                (filter.action_id == 0 || el.action_id == filter.action_id) &&
                (string.IsNullOrEmpty(filter.process_datetime) || el.process_datetime.Date  == dt_process_date.Date) &&
                (string.IsNullOrEmpty(filter.user_name) || el.user_name.Contains(filter.user_name))
            ).ToListAsync();

            return result;
        }


        // POST: api/EventLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EventLog>> PostEventLog(EventLog eventLog)
        {
            _context.EventLogs.Add(eventLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEventLog", new { id = eventLog.id }, eventLog);
        }


        private bool EventLogExists(int id)
        {
            return _context.EventLogs.Any(e => e.id == id);
        }
    }
}
