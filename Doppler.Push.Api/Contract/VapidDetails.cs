namespace Doppler.Push.Api.Contract
{
    public class VapidDetails
    {
        public string Subject { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public long Expiration { get; set; } = -1;
    }
}
