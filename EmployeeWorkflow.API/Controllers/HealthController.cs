using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new { status = "OK", time = DateTime.Now });
        }
    }
}
