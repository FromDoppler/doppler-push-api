namespace Doppler.Push.Api.Contract
{
    public class MessageSendRequest
    {
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationOnClickLink { get; set; }
        public string ImageUrl { get; set; }
        public string IconUrl { get; set; }
    }
}
