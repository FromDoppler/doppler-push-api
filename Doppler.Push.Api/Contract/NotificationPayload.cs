using Newtonsoft.Json;
using System.Collections.Generic;

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

        [JsonProperty("actionEventEndpoints")]
        public Dictionary<string, string> ActionEventEndpoints { get; set; } = new();

        [JsonProperty("actionClickLinks")]
        public Dictionary<string, string> ActionClickLinks { get; set; } = new();
    }

    public class ActionPayload
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
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

        [JsonProperty("actions")]
        public List<ActionPayload> Actions { get; set; }
    }
}
