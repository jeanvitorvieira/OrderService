using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderManager _orderManager;

        public OrdersController(IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _orderManager.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderManager.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateOrderDto dto)
        {
            var created = await _orderManager.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
    }
}