using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orestes.SharedLibrary;
using OzoraSoft.API.Utils.Models;
using OzoraSoft.Library.Security;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OzoraSoft.API.Utils.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagingController: ControllerBase
    {
        private JwtSettings _settings;

        public MessagingController(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        // POST api/compress
        [HttpPost("compress")]
        public Task<Message> Compress([FromBody] Message message)
        {
            return Task.FromResult(new Message()
            {
                Output = UtilsForMessages.Compress(message.Input)
            });
        }

        // POST api/decompress
        [HttpPost("decompress")]
        public Task<Message> Decompress([FromBody] Message message)
        {
            return Task.FromResult(new Message()
            {
                Output = UtilsForMessages.Decompress(message.Input)
            });
        }
        // POST api/encrypt
        [HttpPost("encrypt")]
        public Task<Message> Encrypt([FromBody] Message message)
        {
            return Task.FromResult(new Message()
            {
                Output = UtilsForMessages.Encrypt(_settings.SecretKey, message.Input)
            });
        }

        // POST api/decompress
        [HttpPost("decrypt")]
        public Task<Message> Decrypt([FromBody] Message message)
        {
            return Task.FromResult(new Message()
            {
                Output = UtilsForMessages.Decrypt(_settings.SecretKey,message.Input)
            });
        }
    }
}
