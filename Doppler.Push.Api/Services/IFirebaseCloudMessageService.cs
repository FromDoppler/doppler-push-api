using Doppler.Push.Api.Contract;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public interface IFirebaseCloudMessageService
    {
        Task<FirebaseMessageSendResponse> SendMulticast(PushNotificationDTO request);

        Task<FirebaseMessageSendResponse> SendMulticastAsBatches(PushNotificationDTO request);

        Task<Device> GetDevice(string token);
    }
}
