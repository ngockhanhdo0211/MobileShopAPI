using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShopAPI.Data;
using MobileShopAPI.Models;

namespace MobileShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            return await _context.CartItems.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CartItem>> GetCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return NotFound();
            return cartItem;
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> CreateCartItem(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.Id }, cartItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, CartItem cartItem)
        {
            if (id != cartItem.Id) return BadRequest();

            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

