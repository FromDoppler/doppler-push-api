using Doppler.Push.Api.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class DopplerMessageService : IMessageService
    {
        private readonly IOptions<WebPushSettings> _webPushSettings;
        private readonly IWebPushClient _webPushClient;
        private readonly ILogger<DopplerMessageService> _logger;

        public DopplerMessageService(IOptions<WebPushSettings> webPushSettings, IWebPushClient webPushClient, ILogger<DopplerMessageService> logger)
        {
            _webPushSettings = webPushSettings;
            _webPushClient = webPushClient;

            var settings = _webPushSettings.Value;
            _webPushClient.SetVapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);

            _logger = logger;
        }

        public async Task<MessageSendResponse> SendMulticast(PushNotificationDTO request)
        {
            var allResponses = new List<ResponseItem>();
            var allFailureCount = 0;
            var allSuccessCount = 0;

            // TODO: improve to parallelize shipping
            foreach (var subscription in request.Subscriptions)
            {
                try
                {
                    var serializedPayload = SerializePayload(request, subscription);

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
                        Subscription = subscription != null ?
                            new SubscriptionResponseDTO()
                            {
                                Endpoint = subscription.Endpoint,
                                Auth = subscription.Auth,
                                P256DH = subscription.P256DH,
                            } : null,
                    });
                    allFailureCount += 1;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error sending notification. Subscription: {Subscription}",
                        JsonConvert.SerializeObject(subscription)
                    );
                    allResponses.Add(new ResponseItem()
                    {
                        IsSuccess = false,
                        Exception = new ExceptionItem()
                        {
                            Message = ex.Message,
                            MessagingErrorCode = (int)HttpStatusCode.InternalServerError,
                        },
                        Subscription = subscription != null ?
                            new SubscriptionResponseDTO()
                            {
                                Endpoint = subscription.Endpoint,
                                Auth = subscription.Auth,
                                P256DH = subscription.P256DH,
                            } : null,
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

        private string SerializePayload(PushNotificationDTO pushNotificationDTO, SubscriptionDTO subscriptionDTO)
        {
            var payload = new NotificationPayload
            {
                Title = pushNotificationDTO.NotificationTitle,
                Body = pushNotificationDTO.NotificationBody,
                // TODO: validate correct image and icon urls (https, etc)
                Image = pushNotificationDTO.ImageUrl,
                Icon = pushNotificationDTO.IconUrl,
                Data = new NotificationData()
                {
                    MessageId = pushNotificationDTO.MessageId,
                    ClickLink = pushNotificationDTO.NotificationOnClickLink,
                    ClickedEventEndpoint = subscriptionDTO?.SubscriptionExtraData?.ClickedEventEndpoint,
                    ReceivedEventEndpoint = subscriptionDTO?.SubscriptionExtraData?.ReceivedEventEndpoint,
                    ActionEventEndpoints = subscriptionDTO?.SubscriptionExtraData?.ActionEventEndpoints,
                    ActionClickLinks = MapActionClickLinks(pushNotificationDTO.Actions),
                },
                Actions = MapActions(pushNotificationDTO.Actions),
            };

            return JsonConvert.SerializeObject(payload);
        }

        private List<ActionPayload> MapActions(List<ActionDTO> Actions)
        {
            var result = new List<ActionPayload>();
            if (Actions == null)
            {
                return result;
            }

            foreach (var action in Actions)
            {
                var actionPayload = new ActionPayload()
                {
                    Action = action.Action,
                    Title = action.Title,
                    Icon = action.Icon,
                };

                result.Add(actionPayload);
            }

            return result;
        }

        private Dictionary<string, string> MapActionClickLinks(List<ActionDTO> actions)
        {
            if (actions == null || !actions.Any())
            {
                return null;
            }

            return actions
                .Where(a => !string.IsNullOrEmpty(a.Action) && !string.IsNullOrEmpty(a.Link))
                .ToDictionary(a => a.Action, a => a.Link);
        }
    }
}
