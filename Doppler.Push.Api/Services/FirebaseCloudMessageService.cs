using Doppler.Push.Api.Contract;
using FirebaseAdmin.Messaging;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace Doppler.Push.Api.Services
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
        private readonly FirebaseMessaging _firebaseService;
        public FirebaseCloudMessageService(IConfiguration configuration)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(configuration.GetValue<string>("FirebaseCredencialAdminSdk"))
            });
            _firebaseService = FirebaseMessaging.DefaultInstance;
        }

        public async Task<MessageSendResponse> SendMulticast(MessageSendRequest request)
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

            var response = await _firebaseService.SendMulticastAsync(message);

            var returnResponse = new MessageSendResponse()
            {
                Responses = response.Responses.Select(x => new ResponseItem
                {
                    IsSuccess = x.IsSuccess,
                    MessageId = x.MessageId,
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
    }
}
