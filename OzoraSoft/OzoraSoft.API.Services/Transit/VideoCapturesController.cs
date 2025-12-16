using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OzoraSoft.DataSources;
using OzoraSoft.DataSources.Transit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OzoraSoft.API.Services.Transit
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VideoCapturesController : ControllerBase
    {
        private readonly OzoraSoft_Transit_DBContext _context;

        public VideoCapturesController(OzoraSoft_Transit_DBContext context)
        {
            _context = context;
        }

        // GET: api/VideoCaptures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VideoCapture>>> GetVideoCaptures()
        {
            return await _context.VideoCaptures.ToListAsync();
        }

        // GET: api/VideoCaptures/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoCapture>> GetVideoCapture(int id)
        {
            var videoCapture = await _context.VideoCaptures.FindAsync(id);

            if (videoCapture == null)
            {
                return NotFound();
            }

            return videoCapture;
        }

        // PUT: api/VideoCaptures/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVideoCapture(int id, VideoCapture videoCapture)
        {
            if (id != videoCapture.id)
            {
                return BadRequest();
            }

            _context.Entry(videoCapture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoCaptureExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/VideoCaptures
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VideoCapture>> PostVideoCapture(VideoCapture videoCapture)
        {
            _context.VideoCaptures.Add(videoCapture);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVideoCapture", new { id = videoCapture.id }, videoCapture);
        }

        private bool VideoCaptureExists(int id)
        {
            return _context.VideoCaptures.Any(e => e.id == id);
        }
    }
}
