namespace OrderService.DTOs
{
    public class CreateOrderDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }

    public class InventoryDto
    {
        public int ProductId { get; set; }
        public int Quantidade { get; set; }
    }

    public class DecreaseInventoryRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}