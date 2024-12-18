using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _orderDbContext;

        public OrdersController(OrderDbContext dbContext)
        {
            _orderDbContext = dbContext;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetOrdersAsync()
        {
            var orders = await _orderDbContext.Orders.Include(o => o.Items).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetOrderByIdAsync(int id)
        {
            var order = await _orderDbContext.Orders.Include(o => o.Items)
                                               .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> AddOrderAsync([FromBody] Order order)
        {
            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);
            _orderDbContext.Orders.Add(order);
            await _orderDbContext.SaveChangesAsync();

            var publisher = new RabbitMQPublisher();
            await publisher.PublishAsync(order);

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateOrderByIdAsync(int id, [FromBody] Order updatedOrder)
        {
            var order = await _orderDbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();

            order.CustomerName = updatedOrder.CustomerName;
            order.OrderDate = updatedOrder.OrderDate;
            order.Items = updatedOrder.Items;
            order.TotalAmount = updatedOrder.Items.Sum(i => i.TotalPrice);

            await _orderDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOrderByIdAsync(int id)
        {
            var order = await _orderDbContext.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _orderDbContext.Orders.Remove(order);
            await _orderDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}