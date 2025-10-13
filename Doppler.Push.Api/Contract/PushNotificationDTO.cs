using System.Collections.Generic;

namespace Doppler.Push.Api.Contract
{
    public class PushNotificationDTO
    {
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationOnClickLink { get; set; }
        public string ImageUrl { get; set; }
        public string IconUrl { get; set; }

        public string[] Tokens { get; set; }
        public SubscriptionDTO[] Subscriptions { get; set; }

        // TODO: analyze: this identifier could be used to count deliveries and clicks
        public string MessageId { get; set; }
        public List<ActionDTO> Actions { get; set; }
    }

    public class ActionDTO
    {
        public string Action { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Link { get; set; }
    }

    public class SubscriptionDTO
    {
        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
        public SubscriptionExtraDataDTO SubscriptionExtraData { get; set; }
    }

    public class SubscriptionExtraDataDTO
    {
        public string ClickedEventEndpoint { get; set; }
        public string ReceivedEventEndpoint { get; set; }
        public Dictionary<string, string> ActionEventEndpoints { get; set; } = new();
    }
}
