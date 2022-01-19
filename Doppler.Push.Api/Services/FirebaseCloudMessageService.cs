using Doppler.Push.Api.Contract;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        private readonly FirebaseMessaging _firebaseService;

        public FirebaseCloudMessageService(IOptions<FirebaseCredential> firebaseCredential)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(JsonSerializer.Serialize(firebaseCredential.Value))
            });

            _firebaseService = FirebaseMessaging.DefaultInstance;
        }

        public async Task<FirebaseMessageSendResponse> SendMulticast(FirebaseMessageSendRequest request)
        {
            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Title = request.NotificationTitle,
                    Body = request.NotificationBody,
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

            var returnResponse = new FirebaseMessageSendResponse()
            {
                Responses = response.Responses.Select((x, index) => new FirebaseResponseItem
                {
                    IsSuccess = x.IsSuccess,
                    MessageId = x.MessageId,
                    DeviceToken = request.Tokens[index],
                    Exception = x.IsSuccess ? null : new FirebaseExceptionItem
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

        public async Task<FirebaseMessageSendResponse> SendMulticastAsBatches(FirebaseMessageSendRequest request)
        {
            throw new NotImplementedException();
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
