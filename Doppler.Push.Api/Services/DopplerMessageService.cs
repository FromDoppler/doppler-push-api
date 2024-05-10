using Doppler.Push.Api.Contract;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class DopplerMessageService : IMessageService
    {
        private readonly IOptions<WebPushSettings> _webPushSettings;
        private readonly IWebPushClient _webPushClient;

        public DopplerMessageService(IOptions<WebPushSettings> webPushSettings, IWebPushClient webPushClient)
        {
            _webPushSettings = webPushSettings;
            _webPushClient = webPushClient;

            var settings = _webPushSettings.Value;
            _webPushClient.SetVapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
        }

        public async Task<MessageSendResponse> SendMulticast(PushNotificationDTO request)
        {
            var payload = new NotificationPayload
            {
                Title = request.NotificationTitle,
                Body = request.NotificationBody,
                // TODO: validate correct image and icon urls (https, etc)
                Image = request.ImageUrl,
                Icon = request.IconUrl,
                Data = new NotificationData()
                {
                    MessageId = request.MessageId,
                    ClickLink = request.NotificationOnClickLink,
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
                        },
                        Subscription = subscription,
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
                            // TODO: set Message = ex.Message, and add logging for the current error
                            Message = ex.StackTrace != null ? ex.StackTrace : ex.Message,
                            MessagingErrorCode = (int)HttpStatusCode.InternalServerError,
                        },
                        Subscription = subscription,
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
