using Ais.Common.ServiceRegistration;
using Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace client_webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IConsulClient _consulClient;
        private readonly ConsulRegistrationOptions _consulRegistrationOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public RegistrationController(IConsulClient consulClient, ConsulRegistrationOptions consulRegistrationOptions, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (consulClient is null)
            {
                throw new ArgumentNullException(nameof(consulClient));
            }

            if (consulRegistrationOptions is null)
            {
                throw new ArgumentNullException(nameof(consulRegistrationOptions));
            }

            if (hostApplicationLifetime is null)
            {
                throw new ArgumentNullException(nameof(hostApplicationLifetime));
            }

            _consulClient = consulClient;
            _consulRegistrationOptions = consulRegistrationOptions;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        [HttpPost()]
        public IActionResult Register()
        {
            ConsulRegistrationHelper.Register(_consulRegistrationOptions, _consulClient, _hostApplicationLifetime);

            return Ok();
        }

        public IActionResult Get()
        {
            throw new NotImplementedException();
        }
    }
}
