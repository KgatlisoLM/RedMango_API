using System.ComponentModel.DataAnnotations.Schema;

namespace RedMango_API.Models.Dto
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public MenuItemDto MenuItems { get; set; } = new();
        public int Quantity { get; set; }
        public int ShoppingCartId { get; set; }
    }
}
