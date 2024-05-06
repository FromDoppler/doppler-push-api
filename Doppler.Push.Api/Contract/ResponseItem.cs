namespace Doppler.Push.Api.Contract
{
    public class ResponseItem
    {
        public string MessageId { get; set; }
        public bool IsSuccess { get; set; }
        public ExceptionItem Exception { get; set; }
        public string DeviceToken { get; set; }
        public SubscriptionDTO Subscription { get; set; }
    }
}
