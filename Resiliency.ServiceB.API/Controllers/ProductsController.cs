using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Resiliency.ServiceB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            return Ok(new { Id = id, Name = "Kalem", Price = 100, Stock = 224, Category = "Kalemler" });
        }
    }
}
