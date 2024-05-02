namespace Doppler.Push.Api.Contract
{
    public class FirebaseMessageSendRequest : MessageSendRequest
    {
        public string[] Tokens { get; set; }
    }
}
