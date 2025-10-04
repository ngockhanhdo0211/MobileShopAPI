using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MobileShopAPI.Data;
using MobileShopAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MobileShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ✅ Lấy tất cả user (Admin mới được xem)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // ✅ Lấy user theo id (chính user hoặc admin)
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserName = User.Identity.Name;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Nếu không phải admin và không phải chính user, từ chối
            if (currentUserRole != "Admin" && user.Username != currentUserName)
                return Forbid();

            return user;
        }

        // ✅ Đăng ký (Register) - ai cũng gọi được
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            // Mặc định Role = "User" nếu client không gửi
            user.Role ??= "User";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // ✅ Đăng nhập (Login) -> trả về JWT
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (user == null) return Unauthorized("Sai username hoặc password!");

            // Lấy role từ DB
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        // ✅ Cập nhật user (chính user hoặc admin)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id) return BadRequest();

            var currentUserName = User.Identity.Name;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Nếu không phải admin, không cho sửa Role
            if (currentUserRole != "Admin")
            {
                var originalUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                if (originalUser == null) return NotFound();

                user.Role = originalUser.Role;

                // Chỉ cho phép chính user sửa thông tin của mình
                if (user.Username != currentUserName)
                    return Forbid();
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ Xóa user (Admin mới được xóa)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


