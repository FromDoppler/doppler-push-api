using Doppler.Push.Api.Contract;
using Doppler.Push.Api.DopplerSecurity;
using Doppler.Push.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private IMessageService _firebaseCloudMessageService;
        private IMessageService _dopplerMessageService;

        public MessageController(IMessageServiceFactory messageServiceFactory)
        {
            _firebaseCloudMessageService = messageServiceFactory.CreateFirebaseCloudMessageService();
            _dopplerMessageService = messageServiceFactory.CreateDopplerMessageService();
        }

        [HttpPost]
        public async Task<IActionResult> MessageSend(FirebaseMessageSendRequest messageSend)
        {
            var dto = new PushNotificationDTO()
            {
                NotificationTitle = messageSend.NotificationTitle,
                NotificationBody = messageSend.NotificationBody,
                NotificationOnClickLink = messageSend.NotificationOnClickLink,
                ImageUrl = messageSend.ImageUrl,
                Tokens = messageSend.Tokens,
            };
            var response = await _firebaseCloudMessageService.SendMulticastAsBatches(dto);

            return Ok(response);
        }

        [HttpPost]
        // TODO: analyze remove the authentication and convert the API into an internal API.
        // Related to [DOP-1579](https://makingsense.atlassian.net/browse/DOP-1579)
        [AllowAnonymous]
        [Route("/webpush")]
        public async Task<IActionResult> SendWebPush(PushNotificationRequest pushNotificationRequest)
        {
            if (pushNotificationRequest.Subscriptions == null || pushNotificationRequest.Subscriptions.Length == 0)
            {
                return BadRequest("Subscriptions can not be empty.");
            }

            var dto = new PushNotificationDTO()
            {
                NotificationTitle = pushNotificationRequest.NotificationTitle,
                NotificationBody = pushNotificationRequest.NotificationBody,
                NotificationOnClickLink = pushNotificationRequest.NotificationOnClickLink,
                ImageUrl = pushNotificationRequest.ImageUrl,
                IconUrl = pushNotificationRequest.IconUrl,
                Subscriptions = MapSubscriptions(pushNotificationRequest.Subscriptions),
                MessageId = pushNotificationRequest.MessageId,
            };

            var response = await _dopplerMessageService.SendMulticast(dto);

            return Ok(response);
        }

        private SubscriptionDTO[] MapSubscriptions(Subscription[] subscriptions)
        {
            if (subscriptions == null)
            {
                return null;
            }

            return subscriptions.Select(sub => new SubscriptionDTO
            {
                Endpoint = sub.Endpoint,
                P256DH = sub.P256DH,
                Auth = sub.Auth,
                SubscriptionExtraData = sub.SubscriptionExtraData != null ?
                    new SubscriptionExtraDataDTO()
                    {
                        ClickedEventEndpoint = sub.SubscriptionExtraData.ClickedEventEndpoint,
                        ReceivedEventEndpoint = sub.SubscriptionExtraData.ReceivedEventEndpoint,
                    } : null,
            }).ToArray();
        }
    }
}
