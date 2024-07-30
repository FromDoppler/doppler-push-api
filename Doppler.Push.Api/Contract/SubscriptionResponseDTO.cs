namespace Doppler.Push.Api.Contract
{
    public class SubscriptionResponseDTO
    {
        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
    }
}
