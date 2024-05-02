using Doppler.Push.Api.Contract;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public interface IFirebaseCloudMessageService
    {
        Task<MessageSendResponse> SendMulticast(PushNotificationDTO request);

        Task<MessageSendResponse> SendMulticastAsBatches(PushNotificationDTO request);

        Task<Device> GetDevice(string token);
    }
}
