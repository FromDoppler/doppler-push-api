using System.Collections.Generic;

namespace Doppler.Push.Api.Contract
{
    public class MessageSendResponse
    {
        public IEnumerable<ResponseItem> Responses { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}
