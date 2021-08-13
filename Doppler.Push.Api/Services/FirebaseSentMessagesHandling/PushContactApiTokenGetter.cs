using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Services.FirebaseSentMessagesHandling
{
    public class PushContactApiTokenGetter : IPushContactApiTokenGetter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PushContactApiTokenGetter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _httpContextAccessor.HttpContext.GetTokenAsync("Bearer", "access_token");
        }
    }
}
