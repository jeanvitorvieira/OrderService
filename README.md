# OrderService

Microsserviço responsável por orquestrar a criação de pedidos, integrando com ProductService e InventoryService.

## Tecnologias
- .NET 8
- Entity Framework Core + SQLite
- Swagger/OpenAPI
- HttpClient (comunicação síncrona)

## Porta padrão
`5003`

## Dependências
- **ProductService** rodando em `http://localhost:5001`
- **InventoryService** rodando em `http://localhost:5002`

## Como rodar

```bash
dotnet run
```

Swagger disponível em: http://localhost:5003/swagger

## Endpoints
- `GET /api/orders` — Lista todos os pedidos
- `POST /api/orders` — Cria um novo pedido (valida produto e estoque automaticamente)

## Fluxo de criação de pedido
1. Consulta o ProductService para validar e obter dados do produto
2. Consulta o InventoryService para verificar estoque disponível
3. Se válido, cria o pedido com status `APPROVED` e decrementa o estoque
4. Caso contrário, retorna pedido com status `REJECTED`
