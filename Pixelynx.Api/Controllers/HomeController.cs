using Microsoft.AspNetCore.Mvc;

namespace Pixelynx.Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Get() {
            return Ok();
        }

        [HttpGet("healthz")]
        public IActionResult Health() {
            return Ok();
        }
    }
}
