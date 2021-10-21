using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace client_webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public bool IsFailing => _memoryCache.Get<bool>(nameof(HealthController));

        public HealthController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet]
        public IActionResult Get() => IsFailing ? StatusCode(500) : Ok();

        [HttpPost("set/{failing}")]
        public IActionResult Set(bool failing)
        {
            _memoryCache.Set<bool>(nameof(HealthController), failing);
            return Ok(IsFailing);
        }
    }
}
