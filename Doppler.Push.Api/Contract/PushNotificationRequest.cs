namespace Doppler.Push.Api.Contract
{
    public class PushNotificationRequest : MessageSendRequest
    {
        public Subscription[] Subscriptions { get; set; }
        // TODO: analyze: this identifier could be used to count deliveries and clicks
        public string MessageId { get; set; }
    }

    public class Subscription
    {
        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
        public SubscriptionExtraData SubscriptionExtraData { get; set; }
    }

    public class SubscriptionExtraData
    {
        public string ClickedEventEndpoint { get; set; }
        public string ReceivedEventEndpoint { get; set; }
    }
}
