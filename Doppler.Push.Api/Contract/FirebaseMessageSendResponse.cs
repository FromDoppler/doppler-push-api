using System.Collections.Generic;

namespace Doppler.Push.Api.Contract
{
    public class FirebaseMessageSendResponse
    {
        public IEnumerable<FirebaseResponseItem> Responses { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}
