namespace Doppler.Push.Api.Contract
{
    public class MessageSendRequest
    {
        public string[] Tokens { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationBody { get; set; }
    }
}
