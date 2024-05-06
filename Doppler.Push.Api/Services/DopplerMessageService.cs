using Doppler.Push.Api.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
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

            var allResponses = new List<ResponseItem>();
            var allFailureCount = 0;
            var allSuccessCount = 0;

            // TODO: improve to parallelize shipping
            foreach (var subscription in request.Subscriptions)
            {
                try
                {
                    var response = await _webPushClient.SendNotificationAsync(subscription, serializedPayload);
                    allResponses.Add(response);
                    allSuccessCount += response.IsSuccess ? 1 : 0;
                    allFailureCount += response.IsSuccess ? 0 : 1;
                }
                catch (ArgumentException ex)
                {
                    allResponses.Add(new ResponseItem()
                    {
                        IsSuccess = false,
                        Exception = new ExceptionItem()
                        {
                            Message = ex.Message,
                            MessagingErrorCode = (int)HttpStatusCode.BadRequest,
                        }
                    });
                    allFailureCount += 1;
                }
                catch (Exception ex)
                {
                    allResponses.Add(new ResponseItem()
                    {
                        IsSuccess = false,
                        Exception = new ExceptionItem()
                        {
                            Message = ex.Message,
                            MessagingErrorCode = (int)HttpStatusCode.InternalServerError,
                        }
                    });
                    allFailureCount += 1;
                }
            }

            return new MessageSendResponse()
            {
                Responses = allResponses,
                SuccessCount = allSuccessCount,
                FailureCount = allFailureCount,
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
