using Microsoft.EntityFrameworkCore;

namespace MobileShopAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Precision(18, 2)]   // <-- đặt ở đây mới đúng
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ
        public User? User { get; set; }
    }
}

