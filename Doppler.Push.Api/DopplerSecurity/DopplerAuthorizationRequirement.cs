using Microsoft.AspNetCore.Authorization;

namespace Doppler.Push.Api.DopplerSecurity
{
    public class DopplerAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool AllowSuperUser { get; init; }
    }
}
