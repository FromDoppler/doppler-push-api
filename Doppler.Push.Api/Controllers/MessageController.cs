using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Doppler.Push.Api.Services;
using Doppler.Push.Api.Services.FirebaseSentMessagesHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IFirebaseCloudMessageService _firebaseCloudMessageService;
        private readonly IFirebaseSentMessagesHandler _firebaseSentMessagesHandler;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IFirebaseCloudMessageService firebaseCloudMessageService,
                                IFirebaseSentMessagesHandler firebaseSentMessagesHandler,
                                ILogger<MessageController> logger)
        {
            _firebaseCloudMessageService = firebaseCloudMessageService;
            _firebaseSentMessagesHandler = firebaseSentMessagesHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> MessageSend(FirebaseMessageSendRequest messageSend)
        {
            var response = await _firebaseCloudMessageService.SendMulticast(messageSend);

            try
            {
                await _firebaseSentMessagesHandler.HandleSentMessagesAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error handling sent messages");
            }

            return Ok(response);
        }
    }
}
