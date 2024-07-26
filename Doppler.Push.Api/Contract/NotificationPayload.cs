using Newtonsoft.Json;

namespace Doppler.Push.Api.Contract
{
    public class NotificationData
    {
        // TODO: analyze: this identifier could be used to count deliveries and clicks
        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("clickLink")]
        public string ClickLink { get; set; }

        [JsonProperty("clickedEventEndpoint")]
        public string ClickedEventEndpoint { get; set; }

        [JsonProperty("receivedEventEndpoint")]
        public string ReceivedEventEndpoint { get; set; }
    }

    // Note: all of these values are considered on 'showNotification' in the service worker
    public class NotificationPayload
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("data")]
        public NotificationData Data { get; set; }
    }
}
