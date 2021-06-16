using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Contract
{
    public class MessageSendResponse
    {
        public IEnumerable<ResponseItem> Responses { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }

    public class ResponseItem
    {
        public string MessageId { get; set; }
        public bool IsSuccess { get; set; }
        public ExceptionItem Exception { get; set; }
    }

    public class ExceptionItem
    {
        // Summary:
        //     APNs certificate or web push auth key was invalid or missing.
        //ThirdPartyAuthError = 0,
        //
        //     One or more argument specified in the request was invalid.
        //InvalidArgument = 1,
        //
        //     Internal server error.
        //Internal = 2,
        //
        //     Sending limit exceeded for the message target.
        //QuotaExceeded = 3,
        //
        //     The authenticated sender ID is different from the sender ID for the registration token.
        //SenderIdMismatch = 4,
        //
        //     Cloud Messaging service is temporarily unavailable.
        //Unavailable = 5,
        //
        //     App instance was unregistered from FCM. This usually means that the token used is no longer valid and a new one must be used.
        //Unregistered = 6
        public int MessagingErrorCode { get; set; }
        public string Message { get; set; }
    }
}
