using System.Collections.Generic;

namespace Doppler.Push.Api.Contract
{
    public class FirebaseMessageSendRequest
    {
        public string[] Tokens { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationOnClickLink { get; set; }
        public string ImageUrl { get; set; }
    }
}
