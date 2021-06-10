using Doppler.Push.Api.Contract;
using FirebaseAdmin.Messaging;
using System.Threading.Tasks;
using System.Linq;

namespace Doppler.Push.Api.Services
{
    public class FirebaseCloudMessageService : IFirebaseCloudMessageService
    {
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

            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            var returnResponse = new MessageSendResponse();
            returnResponse.Responses = response.Responses.Select(x => new ResponseItem
            {
                IsSuccess = x.IsSuccess,
                MessageId = x.MessageId,
                Exception = x.IsSuccess ? null : new ExceptionItem
                {
                    Message = x.Exception.Message,
                    MessagingErrorCode = (int?)x.Exception.MessagingErrorCode
                }
            });
            returnResponse.FailureCount = response.FailureCount;
            returnResponse.SuccessCount = response.SuccessCount;

            return returnResponse;
        }
    }
}
