using Doppler.Push.Api.Contract;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services.FirebaseSentMessagesHandling
{
    public class FirebaseSentMessagesHandler : IFirebaseSentMessagesHandler
    {
        private readonly FirebaseSentMessagesHandlerSettings _settings;
        private readonly IPushContactApiTokenGetter _pushContactApiTokenGetter;
        private readonly ILogger<FirebaseSentMessagesHandler> _logger;

        public FirebaseSentMessagesHandler(
            IOptions<FirebaseSentMessagesHandlerSettings> settings,
            IPushContactApiTokenGetter pushContactApiTokenGetter,
            ILogger<FirebaseSentMessagesHandler> logger)
        {
            _settings = settings.Value;
            _pushContactApiTokenGetter = pushContactApiTokenGetter;
            _logger = logger;
        }

        public async Task HandleSentMessagesAsync(FirebaseMessageSendResponse firebaseMessageSendResponse)
        {
            if (firebaseMessageSendResponse == null)
            {
                throw new ArgumentNullException(nameof(firebaseMessageSendResponse));
            }

            if (firebaseMessageSendResponse.Responses == null || !firebaseMessageSendResponse.Responses.Any())
            {
                return;
            }

            var now = DateTime.UtcNow;

            try
            {
                var pushContactHistoryEvents = firebaseMessageSendResponse.Responses
                    .Select(x =>
                    {
                        return new
                        {
                            DeviceToken = x.DeviceToken,
                            SentSuccess = x.IsSuccess,
                            EventDate = now,
                            Details = !x.IsSuccess ? $"{nameof(x.Exception.MessagingErrorCode)} {x.Exception.MessagingErrorCode} - {nameof(x.Exception.Message)} {x.Exception.Message}" : string.Empty
                        };
                    });

                var pushContactApiToken = await _pushContactApiTokenGetter.GetTokenAsync();

                var response = await _settings.PushContactApiUrl
                    .AppendPathSegment("history-events/_bulk")
                    .WithOAuthBearerToken(pushContactApiToken)
                    .SendJsonAsync(HttpMethod.Post, pushContactHistoryEvents);

                if (response.StatusCode != 200)
                {
                    _logger.LogError(@"Error adding following
push contact history events: {@pushContactHistoryEvents}.
Response status code: {@StatusCode}, ", pushContactHistoryEvents, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling following sent messages: {@sentMessages}", firebaseMessageSendResponse.Responses);

                //TODO queue messages to try again
            }

            var sentMessagesWithNotValidDeviceToken = firebaseMessageSendResponse.Responses
            .Where(x => !x.IsSuccess && _settings.FatalMessagingErrorCodes.Any(y => y == x.Exception.MessagingErrorCode));

            if (sentMessagesWithNotValidDeviceToken == null || !sentMessagesWithNotValidDeviceToken.Any())
            {
                return;
            }

            try
            {
                var notValidDeviceTokens = sentMessagesWithNotValidDeviceToken.Select(x => x.DeviceToken);

                var pushContactApiToken = await _pushContactApiTokenGetter.GetTokenAsync();

                var response = await _settings.PushContactApiUrl
                    .AppendPathSegment("push-contacts/_bulk")
                    .WithOAuthBearerToken(pushContactApiToken)
                    .SendJsonAsync(HttpMethod.Delete, notValidDeviceTokens);

                if (response.StatusCode != 200)
                {
                    _logger.LogError(@"Error deleting push contacts with following
device tokens: {@notValidDeviceTokens}.
Response status code: {@StatusCode}, ", notValidDeviceTokens, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling following sent messages: {@sentMessages}", sentMessagesWithNotValidDeviceToken);

                //TODO queue messages to try again
            }
        }
    }
}
