# 🚀 FcgCatalog — CatalogAPI

Microsserviço de **Catálogo de Jogos** da plataforma **FIAP Cloud Games (FCG)**. Faz parte da decomposição em microsserviços orientados a eventos (Tech Challenge FIAP), sendo responsável pelo gerenciamento do catálogo de jogos, criação de pedidos de compra e manutenção da biblioteca dos usuários após a confirmação do pagamento.

---

## 🎯 Responsabilidades

- CRUD de jogos do catálogo.
- Criar pedidos de compra.
- Publicar o evento `OrderPlacedEvent`.
- Consumir `PaymentProcessedEvent` (via Worker).
- Adicionar jogos à biblioteca do usuário após pagamentos aprovados.
- Garantir idempotência por `OrderId` e unicidade `(UserId, GameId)`.

---

## 🔄 Fluxo de Compra

```text
[Usuário] POST /api/orders
            │
            ▼
   CatalogAPI cria Order (PendingPayment)
            │
            └──► OrderPlacedEvent ─► PaymentsAPI
                                          │
                  PaymentProcessedEvent ◄─┘
                            │
                            ▼
                CatalogWorker (Consumer)
                            │
                            ▼
          Order.Approve() + UserGame.Add()
```

---

## 🔌 Endpoints

### Jogos (`/api/games`)

| Verbo | Rota | Autorização | Descrição |
|-------|------|-------------|-----------|
| POST | `/api/games` | Admin | Cria um jogo |
| GET | `/api/games` | Customer | Lista jogos paginados |
| GET | `/api/games/{id}` | Customer | Obtém um jogo |
| PUT | `/api/games/{id}` | Admin | Atualiza um jogo |
| PATCH | `/api/games/{id}/price` | Admin | Atualiza o preço |
| DELETE | `/api/games/{id}` | Admin | Inativa um jogo |

### Pedidos (`/api/orders`)

| Verbo | Rota | Autorização | Descrição |
|-------|------|-------------|-----------|
| POST | `/api/orders` | Customer | Cria um pedido de compra e publica `OrderPlacedEvent` |

### Biblioteca (`/api/library`)

| Verbo | Rota | Autorização | Descrição |
|-------|------|-------------|-----------|
| GET | `/api/library` | Customer | Lista os jogos da biblioteca do usuário autenticado |

---

## 🔧 Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__Default` | Banco PostgreSQL |
| `ConnectionStrings__rabbitmq` | RabbitMQ |
| `JwtSettings__Issuer` | Issuer esperado do JWT |
| `JwtSettings__SecurityKey` | Chave utilizada na validação do JWT |
| `JwtSettings__ExpirationHours` | Tempo de expiração do token |

---

## ▶️ Executando Localmente

### Restaurar dependências

```bash
dotnet restore
```

### Compilar

```bash
dotnet build
```

### Executar a aplicação

```bash
dotnet run --project src/FcgCatalog.AppHost
```

O Aspire AppHost inicia automaticamente a infraestrutura necessária para a aplicação (API, Worker, PostgreSQL e RabbitMQ), desde que o Docker Desktop esteja em execução.

Após a inicialização, acesse o Dashboard do Aspire pela URL exibida no console.

### Executar apenas a API (opcional)

```bash
dotnet run --project src/FcgCatalog.Api
```

> **Observação:** ao executar apenas a API, a infraestrutura (PostgreSQL e RabbitMQ) deve estar disponível.

---

## 🧪 Executando Testes

Todos os testes

```bash
dotnet test
```

Somente testes unitários

```bash
dotnet test tests/FcgCatalog.UnitTests
```

Somente testes de integração

```bash
dotnet test tests/FcgCatalog.IntegrationTests
```

---

## 🏛 Arquitetura

O CatalogAPI foi desenvolvido utilizando:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- Event-Driven Architecture
- MediatR
- FluentValidation

---

## 📋 Tecnologias

- .NET 10
- .NET Aspire
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MassTransit
- MediatR
- FluentValidation
- JWT Authentication
- Serilog
- xUnit
