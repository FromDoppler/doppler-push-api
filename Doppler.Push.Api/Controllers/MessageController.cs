using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Doppler.Push.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private IFirebaseCloudMessageService _firebaseCloudMessageService;

        public MessageController(IFirebaseCloudMessageService firebaseCloudMessageService)
        {
            _firebaseCloudMessageService = firebaseCloudMessageService;
        }

        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost]
        public async Task<IActionResult> MesssageSend(MessageSendRequest messageSend)
        {
            var response = await _firebaseCloudMessageService.SendMulticast(messageSend);

            return Ok(response);
        }
    }
}
