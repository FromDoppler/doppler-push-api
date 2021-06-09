using Doppler.Push.Api.Contract;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services
{
    public interface IFirebaseCloudMessageService
    {
        Task<string> SendMulticast(MessageSendRequest request);
    }
}
