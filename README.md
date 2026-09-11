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
- Utilizar cache distribuído Redis para consultas do catálogo.

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

## 🗃️ Cache Distribuído com Redis

O CatalogAPI utiliza **Redis** como cache distribuído para reduzir consultas repetidas ao PostgreSQL nos endpoints de leitura do catálogo.

### Endpoints com cache

O cache é aplicado em:

```text
GET /api/games
GET /api/games/{id}
```

A estratégia utiliza `StackExchange.Redis` para comunicação com o Redis.

### TTL

Os registros armazenados no cache possuem TTL de:

```text
5 minutos
```

Após esse período, a entrada expira automaticamente e uma nova consulta ao banco pode ser realizada.

### Chaves

As chaves seguem os seguintes padrões:

```text
fcg:catalog:games:v{version}:page:{page}:size:{pageSize}
fcg:catalog:game:{id}
fcg:catalog:games:version
```

A lista utiliza versionamento para permitir a invalidação lógica das diferentes combinações de paginação.

### Invalidação

O cache é invalidado após operações que alteram o catálogo:

```text
POST   /api/games
PUT    /api/games/{id}
PATCH  /api/games/{id}/price
DELETE /api/games/{id}
```

Para alterações, atualização de preço e exclusão, o cache individual do jogo também é removido.

A versão do cache da listagem é incrementada para que as próximas consultas utilizem uma nova chave de versão.

### Cache hit e cache miss

O fluxo de leitura funciona da seguinte forma:

```text
GET /api/games
       │
       ▼
   Consulta Redis
       │
   ┌───┴────┐
   │        │
 HIT       MISS
   │        │
   ▼        ▼
 Retorna  PostgreSQL
 cache      │
            ▼
       Salva no Redis
            │
            ▼
          Retorna
```

O mesmo conceito é aplicado ao endpoint:

```text
GET /api/games/{id}
```

### Indisponibilidade do Redis

O cache foi implementado de forma **fail-open**.

Caso o Redis esteja indisponível:

```text
Cliente
   │
   ▼
CatalogAPI
   │
   ├── Redis indisponível
   │
   ▼
PostgreSQL
   │
   ▼
Resposta
```

A indisponibilidade do Redis não impede o funcionamento principal da API.

Falhas de leitura e escrita no Redis são tratadas e registradas em log, permitindo que a aplicação continue utilizando o banco de dados como fonte principal.

---

## 🔧 Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__Default` | Banco PostgreSQL |
| `ConnectionStrings__rabbitmq` | RabbitMQ |
| `ConnectionStrings__redis` | Redis utilizado pelo cache distribuído |
| `JwtSettings__Issuer` | Issuer esperado do JWT |
| `JwtSettings__SecurityKey` | Chave utilizada na validação do JWT |
| `JwtSettings__ExpirationHours` | Tempo de expiração do token |

### Redis no ambiente local

Quando executado através do .NET Aspire, o Redis é provisionado pelo AppHost e disponibilizado para a API através do recurso:

```text
redis
```

Quando executado através do Docker Compose:

```text
redis:6379
```

Quando executado no Kubernetes:

```text
fcg-redis:6379
```

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

O Aspire AppHost inicia automaticamente a infraestrutura necessária para a aplicação (API, Worker, PostgreSQL, RabbitMQ e Redis), desde que o Docker Desktop esteja em execução.

Após a inicialização, acesse o Dashboard do Aspire pela URL exibida no console.

### Executar apenas a API (opcional)

```bash
dotnet run --project src/FcgCatalog.Api
```

> **Observação:** ao executar apenas a API, a infraestrutura (PostgreSQL, RabbitMQ e Redis) deve estar disponível.

---

## 🐳 Docker Compose

A execução integrada através do repositório de plataforma utiliza Docker Compose para provisionar a infraestrutura compartilhada.

O Redis é executado através da imagem:

```text
redis:7-alpine
```

A API do catálogo utiliza:

```text
ConnectionStrings__redis=redis:6379
```

Para mais informações sobre a execução integrada da plataforma, consulte a documentação do repositório **FCG Platform**.

---

## ☸️ Kubernetes

No ambiente Kubernetes, o CatalogAPI utiliza o Redis disponibilizado pelo Service:

```text
fcg-redis:6379
```

A conexão é configurada através de:

```text
ConnectionStrings__redis
```

A imagem utilizada no ambiente Kubernetes é:

```text
jondamiao/fcg-catalog-api:1.0.2
```

O Redis e o CatalogAPI devem estar no namespace:

```text
fcg-platform
```

---

## 🧪 Executando Testes

Todos os testes:

```bash
dotnet test
```

Somente testes unitários:

```bash
dotnet test tests/FcgCatalog.UnitTests
```

Somente testes de integração:

```bash
dotnet test tests/FcgCatalog.IntegrationTests
```

### Testes de cache

Os testes de cache estão concentrados em:

```text
tests/FcgCatalog.IntegrationTests/Endpoints/GamesCacheTests.cs
```

São validados cenários como:

- Consultas consecutivas.
- Cache de listagem.
- Cache por ID.
- Atualização de preço.
- Atualização do jogo.
- Exclusão do jogo.
- Criação de novo jogo.
- Invalidação do cache após operações de escrita.

Também existem testes unitários dos handlers responsáveis pelas operações de leitura e invalidação do cache.

### Testes de performance

O projeto possui testes específicos para avaliar o comportamento do cache:

```text
tests/FcgCatalog.PerformanceTests
```

A comparação realizada considera uma primeira consulta com **cache miss** e consultas posteriores com **cache hit**.

Resultados obtidos durante a validação:

| Métrica | Resultado |
|---------|-----------|
| Cache MISS | 453,16 ms |
| Cache HIT — 20 requisições | 132,30 ms |
| Cache HIT — média | 6,62 ms/requisição |
| Redução observada | 98,54% |

---

## 🏛 Arquitetura

O CatalogAPI foi desenvolvido utilizando:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- Event-Driven Architecture
- MediatR
- FluentValidation
- Cache distribuído

A implementação do cache utiliza uma abstração própria:

```text
IGameCacheService
        │
        ▼
RedisGameCacheService
        │
        ▼
StackExchange.Redis
        │
        ▼
Redis
```

O PostgreSQL permanece como fonte principal dos dados. O Redis atua como camada temporária de cache para consultas de leitura.

---

## 📋 Tecnologias

- .NET 10
- .NET Aspire
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Redis 7
- StackExchange.Redis
- MassTransit
- MediatR
- FluentValidation
- JWT Authentication
- Serilog
- xUnit
