using Doppler.Push.Api.Contract;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class DopplerMessageService : IMessageService
    {
        private IWebPushClient _webPushClient;

        public DopplerMessageService()
        {
            // TODO: obtains these values from config
            var subject = "https://prueba.com";
            var publicKey = "REPLACE_WITH_PUBLIC_KEY";
            var privateKey = "REPLACE_WITH_PRIVATE_KEY";

            _webPushClient = new WebPushClient();
            _webPushClient.SetVapidDetails(subject, publicKey, privateKey);
        }

        public async Task<MessageSendResponse> SendMulticast(PushNotificationDTO request)
        {
            var payload = new NotificationPayload
            {
                Title = request.NotificationTitle,
                Body = request.NotificationBody,
                // TODO: replace properly
                Icon = "https://png.pngtree.com/element_origin_min_pic/16/08/05/1057a3fae73b91b.jpg",
                // TODO: receive proper data and replace
                Data = new NotificationData()
                {
                    MessageId = "test-api-3333",
                },
            };

            // Serializar el objeto a JSON
            string serializedPayload = JsonConvert.SerializeObject(payload);

            foreach (var subscription in request.Subscriptions)
            {
                await _webPushClient.SendNotificationAsync(subscription, serializedPayload);

                //allResponses.AddRange(response.Responses);
                //allFailureCount += response.FailureCount;
                //allSuccessCount += response.SuccessCount;
            }

            return new MessageSendResponse()
            {
                SuccessCount = 0,
                FailureCount = 0,
            };
        }

        public async Task<MessageSendResponse> SendMulticastAsBatches(PushNotificationDTO request)
        {
            await Task.Yield(); // it allows us to consider an async method without doing an operation
            throw new Exception("Not implemented");
        }

        public async Task<Device> GetDevice(string token)
        {
            await Task.Yield(); // it allows us to consider an async method without doing an operation
            throw new Exception("Not implemented");
        }
    }
}
