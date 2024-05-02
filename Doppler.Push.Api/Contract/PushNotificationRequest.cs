namespace Doppler.Push.Api.Contract
{
    public class PushNotificationRequest : MessageSendRequest
    {
        public Subscription[] Subscriptions { get; set; }
    }

    public class Subscription
    {
        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
    }
}
