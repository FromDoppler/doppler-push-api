using System.Collections.Generic;

namespace Doppler.Push.Api.Services.FirebaseSentMessagesHandling
{
    public class FirebaseSentMessagesHandlerSettings
    {
        public string PushContactApiUrl { get; set; }
        public List<int> FatalMessagingErrorCodes { get; set; }
    }
}
