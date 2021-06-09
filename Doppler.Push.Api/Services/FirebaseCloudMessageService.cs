using Doppler.Push.Api.Contract;
using FirebaseAdmin.Messaging;
using System.Threading.Tasks;
using System.Text.Json;

namespace Doppler.Push.Api.Services
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        public async Task<string> SendMulticast(MessageSendRequest request)
        {
            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Title = request.NotificationTitle,
                    Body = request.NotificationBody,
                },
                Tokens = request.Tokens,
            };

            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            return JsonSerializer.Serialize(response);
        }
    }
}
