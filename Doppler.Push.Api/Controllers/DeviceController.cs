using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Doppler.Push.Api.Services;
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
        private readonly IFirebaseCloudMessageService _firebaseCloudMessageService;

        public DeviceController(IFirebaseCloudMessageService firebaseCloudMessageService)
        {
            _firebaseCloudMessageService = firebaseCloudMessageService;
        }

        [HttpGet]
        [Route("devices/{token}")]
        public async Task<ActionResult<Device>> Get([FromRoute] string token)
        {
            var device = await _firebaseCloudMessageService.GetDevice(token);

            return new OkObjectResult(device);
        }
    }
}
