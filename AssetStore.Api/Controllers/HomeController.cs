using Microsoft.AspNetCore.Mvc;

namespace AssetStore.Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Get() 
        {
            return Ok();
        }

        [HttpGet("health")]
        public IActionResult Health() 
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Upload()
        {
            return Ok();
        }
    }
}
