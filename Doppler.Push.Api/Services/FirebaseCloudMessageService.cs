using Doppler.Push.Api.Contract;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        private readonly FirebaseMessaging _firebaseService;
        private readonly FirebaseCloudMessageServiceSettings _firebaseCloudMessageServiceSettings;

        public FirebaseCloudMessageService(IOptions<FirebaseCredential> firebaseCredential, IOptions<FirebaseCloudMessageServiceSettings> firebaseCloudMessageServiceSettings)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(JsonSerializer.Serialize(firebaseCredential.Value))
            });

            _firebaseService = FirebaseMessaging.DefaultInstance;

            _firebaseCloudMessageServiceSettings = firebaseCloudMessageServiceSettings.Value;
        }

        public async Task<MessageSendResponse> SendMulticast(PushNotificationDTO request)
        {
            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Title = request.NotificationTitle,
                    Body = request.NotificationBody,
                    ImageUrl = request.ImageUrl
                },
                Tokens = request.Tokens,
                Webpush = !string.IsNullOrEmpty(request.NotificationOnClickLink) ?
                new WebpushConfig
                {
                    FcmOptions = new WebpushFcmOptions()
                    {
                        Link = request.NotificationOnClickLink
                    }
                } : null,
            };

            var response = await _firebaseService.SendMulticastAsync(message);

            var returnResponse = new MessageSendResponse()
            {
                Responses = response.Responses.Select((x, index) => new ResponseItem
                {
                    IsSuccess = x.IsSuccess,
                    MessageId = x.MessageId,
                    DeviceToken = request.Tokens[index],
                    Exception = x.IsSuccess ? null : new ExceptionItem
                    {
                        Message = x.Exception.Message,
                        MessagingErrorCode = x.Exception.MessagingErrorCode.HasValue ? (int)x.Exception.MessagingErrorCode : 0
                    }
                }),
                FailureCount = response.FailureCount,
                SuccessCount = response.SuccessCount
            };

            return returnResponse;
        }

        public async Task<MessageSendResponse> SendMulticastAsBatches(PushNotificationDTO request)
        {
            var requestsBatches = request.Tokens
                .Batch(_firebaseCloudMessageServiceSettings.BatchesSize) // TODO: replace with Enumerable.Chunk after NET 6 migration https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.chunk?view=net-6.0
                .Select(x =>
                    new PushNotificationDTO
                    {
                        Tokens = x as string[],
                        NotificationTitle = request.NotificationTitle,
                        NotificationBody = request.NotificationBody,
                        NotificationOnClickLink = request.NotificationOnClickLink,
                        ImageUrl = request.ImageUrl,
                    });

            // TODO: refactor to use a declarative implementation instead of mutable variables
            var allResponses = new List<ResponseItem>();
            var allFailureCount = 0;
            var allSuccessCount = 0;

            foreach (var currentRequest in requestsBatches)
            {
                var response = await SendMulticast(currentRequest);

                allResponses.AddRange(response.Responses);
                allFailureCount += response.FailureCount;
                allSuccessCount += response.SuccessCount;
            }

            return new MessageSendResponse
            {
                Responses = allResponses,
                FailureCount = allFailureCount,
                SuccessCount = allSuccessCount
            };
        }

        public async Task<Device> GetDevice(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or empty.", nameof(token));
            }

            var dummyMessage = new Message
            {
                Notification = new Notification()
                {
                    Title = "dummy title",
                    Body = "dummy body",
                },
                Token = token
            };

            try
            {
                await _firebaseService.SendAsync(dummyMessage, true);
            }
            catch (FirebaseMessagingException)
            {
                return new Device
                {
                    Token = token,
                    IsValid = false
                };
            }

            return new Device
            {
                Token = token,
                IsValid = true
            };
        }
    }
}
