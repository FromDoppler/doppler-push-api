namespace Doppler.Push.Api.Services
{
    public interface IMessageServiceFactory
    {
        IMessageService CreateFirebaseCloudMessageService();
        IMessageService CreateDopplerMessageService();
    }
}
