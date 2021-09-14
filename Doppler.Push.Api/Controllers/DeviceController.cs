using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    public class DeviceController
    {
        [HttpGet]
        [Route("devices/{token}")]
        public async Task<ActionResult<Device>> Get([FromRoute] string token)
        {
            throw new NotImplementedException();
        }
    }
}
