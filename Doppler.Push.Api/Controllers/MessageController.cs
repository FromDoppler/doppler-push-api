using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Doppler.Push.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private IFirebaseCloudMessageService _firebaseCloudMessageService;

        public MessageController(IFirebaseCloudMessageService firebaseCloudMessageService)
        {
            _firebaseCloudMessageService = firebaseCloudMessageService;
        }

        [HttpPost]
        public async Task<IActionResult> MessageSend(FirebaseMessageSendRequest messageSend)
        {
            var response = await _firebaseCloudMessageService.SendMulticastAsBatches(messageSend);

            return Ok(response);
        }
    }
}
