using Newtonsoft.Json;

namespace Doppler.Push.Api.Contract
{
    public class NotificationData
    {
        // TODO: analyze: this identifier could be used to count deliveries and clicks
        [JsonProperty("messageId")]
        public string MessageId { get; set; }
    }

    public class NotificationPayload
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("data")]
        public NotificationData Data { get; set; }
    }
}
