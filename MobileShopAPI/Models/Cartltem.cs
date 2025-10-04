namespace MobileShopAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        // Quan hệ
        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
