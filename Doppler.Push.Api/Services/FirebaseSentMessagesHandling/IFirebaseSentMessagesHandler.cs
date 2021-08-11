using Doppler.Push.Api.Contract;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services.FirebaseSentMessagesHandling
{
    public interface IFirebaseSentMessagesHandler
    {
        Task HandleSentMessagesAsync(FirebaseMessageSendResponse firebaseMessageSendResponse);
    }
}
