# 🚀 FcgCatalog — CatalogAPI

Microsserviço de **Catálogo de Jogos** da plataforma **FIAP Cloud Games (FCG)**. Faz parte da decomposição em microsserviços orientados a eventos (Fase 2 do Tech Challenge), substituindo a parte de jogos do monolito original.

## 🎯 Responsabilidades

- CRUD de jogos do catálogo (criar, listar, obter, atualizar, alterar preço, inativar).
- Iniciar o fluxo de compra publicando **`OrderPlacedEvent`**.
- Consumir **`PaymentProcessedEvent`** (via Worker) e adicionar o jogo à biblioteca do usuário quando o pagamento for `Approved`. Pagamentos `Rejected` não alteram a biblioteca.
- Manter a biblioteca por usuário (`UserGame`), com idempotência por `OrderId` e unicidade `(UserId, GameId)`.

## 🔄 Fluxo de Compra

```
[Usuário] POST /api/orders
            │
            ▼
   CatalogAPI cria Order(PendingPayment)
            │
            └──► OrderPlacedEvent ─► [PaymentsAPI]
                                          │
                  PaymentProcessedEvent ◄─┘
                            │
                            ▼
              CatalogWorker (PaymentProcessedConsumer)
                            │
                            ▼
            Order.Approve()  +  UserGame.Add()      (idempotente)
```

## 🔌 Endpoints — `/api/games`

| Verbo  | Rota                          | Policy           | Descrição                          |
| ------ | ----------------------------- | ---------------- | ---------------------------------- |
| POST   | `/api/games`                  | `AdminPolicy`    | Cria um novo jogo                  |
| GET    | `/api/games?Page&PageSize`    | `CustomerPolicy` | Lista paginada de jogos ativos     |
| GET    | `/api/games/{id}`             | `CustomerPolicy` | Obtém um jogo por identificador    |
| PUT    | `/api/games/{id}`             | `AdminPolicy`    | Atualiza dados do jogo             |
| PATCH  | `/api/games/{id}/price`       | `AdminPolicy`    | Altera o preço de um jogo          |
| DELETE | `/api/games/{id}`             | `AdminPolicy`    | Inativa um jogo (soft delete)      |

## 🛒 Endpoints — `/api/library`

| Verbo | Rota                       | Policy           | Descrição                                                         |
| ----- | -------------------------- | ---------------- | ----------------------------------------------------------------- |
| POST  | `/api/orders`              | `CustomerPolicy` | Inicia a compra de um jogo (publica `OrderPlacedEvent`)           |
| GET   | `/api/library`             | `CustomerPolicy` | Lista a biblioteca do usuário autenticado                         |

## 🔧 Variáveis de Ambiente

| Variável                            | Descrição                                                |
| ----------------------------------- | -------------------------------------------------------- |
| `ConnectionStrings__Default`        | Connection string do PostgreSQL do CatalogAPI            |
| `ConnectionStrings__rabbitmq`       | Connection string do RabbitMQ (publisher de eventos)     |
| `JwtSettings__Issuer`               | Issuer esperado no JWT emitido pelo UsersAPI             |
| `JwtSettings__SecurityKey`          | Chave simétrica utilizada para validar o JWT             |
| `JwtSettings__ExpirationHours`      | Tempo de expiração (utilizado apenas para testes)        |

> Em produção (Kubernetes), `ConnectionStrings__*` e `JwtSettings__SecurityKey` ficam em `Secret`; demais opções em `ConfigMap`.

---

Template original do projeto (ArchForge):

## 📑 Sumário

- [📋 Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [🏛 Arquitetura](#-arquitetura)
- [📁 Estrutura da Solução](#-estrutura-da-solução)
- [▶️ Executando Localmente](#️-executando-localmente)
- [🗄 Banco de Dados](#-banco-de-dados)
- [🔐 Autenticação JWT](#-autenticação-jwt)
  - [Gerando Token Manualmente](#gerando-token-manualmente)
  - [Policies Disponíveis](#policies-disponíveis)
- [📡 Coleção Postman](#-coleção-postman)
- [🧪 Executando Testes](#-executando-testes)
- [📚 Documentação](#-documentação)
  - [ADRs](#adrs)
  - [Diagramas](#diagramas)
  - [Linguagem Ubíqua](#linguagem-ubíqua)
- [🎯 Objetivos do Template](#-objetivos-do-template)

## 📋 Tecnologias Utilizadas

- .NET 10
- ASP.NET Core Minimal API
- .NET Aspire
- MediatR
- FluentValidation
- Mapperly
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Health Checks
- Serilog
- xUnit
- Shouldly
- NSubstitute
- Aspire Testing
- Respawn

## 🏛 Arquitetura

Este template segue princípios de:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- SOLID
- Separation of Concerns

### Camadas

| Projeto                    | Responsabilidade                                       |
| -------------------------- | ------------------------------------------------------ |
| FcgCatalog.Api             | Endpoints, Middlewares e Configurações                 |
| FcgCatalog.Application     | Casos de uso, Commands, Queries, Validators e Handlers |
| FcgCatalog.Domain          | Entidades, Regras de Negócio e Contratos               |
| FcgCatalog.Infrastructure  | Persistência, EF Core e Repositórios                   |
| FcgCatalog.IoC             | Registro de dependências                               |
| FcgCatalog.SharedKernel    | Componentes compartilhados                             |
| FcgCatalog.ServiceDefaults | Configurações compartilhadas Aspire                    |
| FcgCatalog.AppHost         | Orquestração Aspire                                    |

## 📁 Estrutura da Solução

```text
src/
├── FcgCatalog.Api
├── FcgCatalog.AppHost
├── FcgCatalog.Application
├── FcgCatalog.Domain
├── FcgCatalog.Infrastructure
├── FcgCatalog.IoC
├── FcgCatalog.ServiceDefaults
└── FcgCatalog.SharedKernel

tests/
├── FcgCatalog.UnitTests
└── FcgCatalog.IntegrationTests

docs/
├── adrs
├── api-collection
├── diagrams
└── linguagem-ubiqua
```

## ▶️ Executando Localmente

### Restaurar dependências

```bash
dotnet restore
```

### Compilar

```bash
dotnet build
```

### Executar com Aspire

```bash
dotnet run --project src/FcgCatalog.AppHost
```

### Executar apenas a API

```bash
dotnet run --project src/FcgCatalog.Api
```

## 🗄 Banco de Dados

Criar migration:

```bash
dotnet ef migrations add MinhaMigration -p src/FcgCatalog.Infrastructure -s src/FcgCatalog.Api --output-dir Database/Migrations
```

Aplicar migrations:

```bash
dotnet ef database update -p src/FcgCatalog.Infrastructure -s src/FcgCatalog.Api
```

## 🔐 Autenticação JWT

O template já possui autenticação JWT configurada.

Configuração padrão:

```json
"JwtSettings": {
    "Issuer": "FcgCatalog-Issuer",
    "SecurityKey": "FcgCatalog_Secret_Key_2026_High_Security_Token",
    "ExpirationHours": 2
}
```

### Gerando Token Manualmente

Acesse:

🌐 https://jwt.io

#### Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### Payload para perfil Admin

```json
{
  "iss": "FcgCatalog-Issuer",
  "sub": "1",
  "name": "Administrador",
  "role": "Admin",
  "exp": 1893456000
}
```

#### Payload para perfil Customer

```json
{
  "iss": "FcgCatalog-Issuer",
  "sub": "2",
  "name": "Cliente",
  "role": "Customer",
  "exp": 1893456000
}
```

#### Secret

```text
FcgCatalog_Secret_Key_2026_High_Security_Token
```

Após gerar o token, utilize:

```http
Authorization: Bearer {TOKEN}
```

### Policies Disponíveis

| Policy         | Roles Permitidas |
| -------------- | ---------------- |
| CustomerPolicy | Admin, Customer  |
| AdminPolicy    | Admin            |

## 📡 Coleção Postman

A coleção da API está disponível em:

```text
docs/api-collection/
```

Importe o arquivo `.json` no Postman para iniciar os testes rapidamente.

## 🧪 Executando Testes

### Todos os testes

```bash
dotnet test
```

### Unitários

```bash
dotnet test tests/FcgCatalog.UnitTests
```

### Integração

```bash
dotnet test tests/FcgCatalog.IntegrationTests
```

## 📊 Cobertura de Testes

O template já possui suporte à geração de cobertura de testes utilizando Coverlet.

### Gerar cobertura dos testes unitários

```bash
dotnet test tests/FcgCatalog.UnitTests --collect:"XPlat Code Coverage" --settings .runsettings
```

### Instalar o ReportGenerator

Caso ainda não possua a ferramenta instalada:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Gerar relatório HTML

```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;MarkdownSummary"
```

### Visualizar relatório

Abra o arquivo:

```text
coverage-report/index.html
```

## 📚 Documentação

A documentação do projeto fica centralizada na pasta:

```text
docs/
```

### ADRs

```text
docs/adrs
```

Registro das decisões arquiteturais.

### Diagramas

```text
docs/diagrams
```

Diagramas de arquitetura e fluxo.

### Linguagem Ubíqua

```text
docs/linguagem-ubiqua
```

Glossário do domínio.

## 🎯 Objetivos do Template

Este template foi criado para fornecer:

- Estrutura pronta
- Padronização entre serviços
- Alta cobertura de testes
- Baixo tempo de setup
- Facilidade de manutenção
- Evolução arquitetural consistente

Gerado com ❤️ utilizando ArchForge.
