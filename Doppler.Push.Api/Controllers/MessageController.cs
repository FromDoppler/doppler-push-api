using Doppler.Push.Api.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Doppler.Push.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost]
        public async Task<IActionResult> MesssageSend(string deviceToken)
        {
            return Ok($"Hello! you have a valid SuperUser! - Device Token {deviceToken}");
        }
    }
}
