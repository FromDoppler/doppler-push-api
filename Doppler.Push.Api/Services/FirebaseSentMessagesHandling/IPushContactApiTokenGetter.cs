using System.Threading.Tasks;

namespace Doppler.Push.Api.Services.FirebaseSentMessagesHandling
{
    public interface IPushContactApiTokenGetter
    {
        Task<string> GetTokenAsync();
    }
}
