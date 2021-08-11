namespace Doppler.Push.Api.Contract
{
    public class FirebaseResponseItem
    {
        public string MessageId { get; set; }
        public bool IsSuccess { get; set; }
        public FirebaseExceptionItem Exception { get; set; }
    }
}
