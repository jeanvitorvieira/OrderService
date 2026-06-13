using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IOrderManager
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<Order> CreateOrderAsync(CreateOrderDto dto);
    }

    public class OrderManager : IOrderManager
    {
        private readonly IOrderRepository _repository;
        private readonly HttpClient _httpClient;

        public OrderManager(IOrderRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Order>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Order?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Order
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow,
                Status = "PENDING"
            };

            // Passo 1: Consultar ProductService (Porta 5001)
            var productResponse = await _httpClient.GetAsync($"http://localhost:5001/api/products/{dto.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
            {
                order.Status = "REJECTED";
                order.ProductName = "Desconhecido";
                return await _repository.AddAsync(order);
            }

            var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (product == null)
            {
                order.Status = "REJECTED";
                return await _repository.AddAsync(order);
            }

            order.ProductName = product.Nome;
            order.UnitPrice = product.Preco;
            order.TotalPrice = product.Preco * dto.Quantity;

            // Passo 2: Consultar InventoryService (Porta 5002)
            var inventoryResponse = await _httpClient.GetAsync($"http://localhost:5002/api/inventory/{dto.ProductId}");
            if (!inventoryResponse.IsSuccessStatusCode)
            {
                order.Status = "REJECTED";
                return await _repository.AddAsync(order);
            }

            var inventory = await inventoryResponse.Content.ReadFromJsonAsync<InventoryDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (inventory == null || inventory.Quantidade < dto.Quantity)
            {
                order.Status = "REJECTED";
                return await _repository.AddAsync(order);
            }

            // Passo 3: Solicitar baixa de estoque
            var decreaseRequest = new DecreaseInventoryRequest { ProductId = dto.ProductId, Quantity = dto.Quantity };
            var decreaseResponse = await _httpClient.PutAsJsonAsync("http://localhost:5002/api/inventory/decrease", decreaseRequest);

            if (!decreaseResponse.IsSuccessStatusCode)
            {
                order.Status = "REJECTED";
                return await _repository.AddAsync(order);
            }

            // Passo 4: Salvar pedido
            order.Status = "APPROVED";
            return await _repository.AddAsync(order);
        }
    }
}