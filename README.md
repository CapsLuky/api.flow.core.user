# CoreUser API

API de sincronização e orquestração de usuários multi-empresas com integração ao [Clerk.com](https://clerk.com/) para autenticação e gerenciamento centralizado.

## 📋 Sobre o Projeto

O **CoreUser** é uma solução robusta desenvolvida em .NET 9 que sincroniza automaticamente usuários entre a plataforma Clerk e bancos de dados internos de múltiplas empresas. A API utiliza webhooks para manter os dados sempre atualizados em tempo real, garantindo consistência entre sistemas.

### Objetivo Principal

Sincronizar e orquestrar usuários de múltiplas empresas com banco de dados interno, mantendo a integridade dos dados através de eventos em tempo real provenientes do Clerk via webhooks Svix.

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, organizado em três camadas principais:

```
CoreUser/
├── Api/                    # Camada de Apresentação (Minimal API)
│   ├── Endpoints/          # Definição de rotas e handlers
│   ├── Middleware/         # Middlewares customizados
│   └── Modules/            # Módulos de configuração
│
├── Core/                   # Camada de Domínio (Lógica de Negócio)
│   ├── ClerkWebhook/       # Processamento de webhooks do Clerk
│   │   ├── Services/       # Serviços de domínio
│   │   ├── Models/         # Entidades e DTOs
│   │   └── Validators/     # Validações de negócio
│   ├── Client/             # Gestão de usuários
│   └── Common/             # Estruturas compartilhadas
│
└── Infrastructure/         # Camada de Infraestrutura
    ├── ClerkWebhook/       # Repositórios de webhook
    ├── Client/             # Repositórios de usuário
    └── Statics/            # Configurações estáticas
```

### Princípios Arquiteturais

- **Separation of Concerns**: Cada camada tem responsabilidades bem definidas
- **Dependency Inversion**: Dependências apontam para abstrações, não implementações
- **Single Responsibility**: Classes e métodos com propósito único e claro
- **Clean Code**: Código legível, testável e manutenível

## 🚀 Tecnologias e Ferramentas

### Framework e Runtime
- **.NET 9.0** - Framework moderno e performático
- **ASP.NET Core Minimal API** - APIs leves e de alta performance

### Banco de Dados
- **MongoDB 3.4.2** - Banco NoSQL para armazenamento flexível de dados

### Autenticação e Webhooks
- **Clerk.Net 1.15.0** - SDK oficial do Clerk para .NET
- **Clerk.Webhooks 0.0.2** - Processamento de webhooks
- **Svix** - Verificação de assinaturas de webhooks

### Validação e Qualidade
- **FluentValidation 12.0.0** - Validações fluentes e expressivas

### Logging e Observabilidade
- **Serilog 9.0.0** - Logging estruturado e configurável
- **Serilog.Expressions** - Templates avançados de log

### Documentação
- **Scalar.AspNetCore 2.6.4** - Documentação interativa moderna
- **Swashbuckle** - Suporte adicional ao Swagger/OpenAPI

## ✨ Funcionalidades Principais

### 1. Processamento de Webhooks do Clerk

A API recebe e processa eventos em tempo real do Clerk através de webhooks seguros:

- **Criação de Usuário** (`user.created`): Sincroniza novos usuários automaticamente
- **Atualização de Usuário** (`user.updated`): Mantém dados atualizados
- **Exclusão de Usuário** (`user.deleted`): Remove usuários do sistema

#### Segurança de Webhooks
- Verificação de assinatura Svix para garantir autenticidade
- Validação de headers obrigatórios (`svix-id`, `svix-timestamp`, `svix-signature`)
- Suporte a múltiplas aplicações com secrets independentes

### 2. Gestão Multi-Tenant

Suporte nativo para múltiplas empresas/aplicações:

```csharp
// Identificação automática da aplicação via header
request.Headers.TryGetValue("application_id", out var applicationId);

// Roteamento para o secret correto
var clerkApplication = applicationId == "comgas" 
    ? ClerkApplication.Comgas 
    : ClerkApplication.MultiTenant;
```

### 3. API de Consulta de Usuários

Endpoints RESTful para gerenciamento de usuários:

- `GET /client/user` - Lista todos os usuários
- `GET /client/user/{id}` - Busca usuário por ID
- `POST /client/user` - Criação de usuário (mock)

### 4. Logging Estruturado

Sistema completo de logs com Serilog:

- Logs estruturados em JSON
- Rastreamento de requisições HTTP
- Contexto enriquecido com trace/span IDs
- Logs específicos para webhooks

## 🎯 Minimal API - Vantagens

O projeto utiliza **Minimal APIs** do ASP.NET Core, oferecendo:

### Performance
- Menos overhead comparado a Controllers tradicionais
- Inicialização mais rápida da aplicação
- Menor consumo de memória

### Simplicidade
```csharp
// Definição clara e concisa de endpoints
webhooks.MapPost("/clerk", ProcessClerkWebhook)
    .WithName("ProcessClerkWebhook")
    .WithSummary("Processar webhook do Clerk")
    .Produces<string>(200)
    .Produces<string>(400);
```

### Manutenibilidade
- Código mais legível e direto
- Menos boilerplate
- Fácil de testar e documentar

### Organização
- Endpoints agrupados logicamente em classes estáticas
- Separação clara de responsabilidades
- Fácil navegação no código

## 🔧 Configuração

### Pré-requisitos

- .NET 9.0 SDK
- MongoDB 3.4+
- Conta no [Clerk.com](https://clerk.com/)

## 📡 Endpoints

### Webhooks

#### POST `/api/webhooks/clerk`
Processa webhooks do Clerk

**Headers Obrigatórios:**
- `application_id`: Identificador da aplicação
- `svix-id`: ID único do evento
- `svix-timestamp`: Timestamp do evento
- `svix-signature`: Assinatura de verificação

**Resposta:**
```json
{
  "message": "Webhook processado com sucesso",
  "timestamp": "2024-12-02T10:30:00Z"
}
```

Busca usuário específico por ID

## 🧪 Testes

```bash
# Execute os testes unitários
dotnet test Unit.Test/
```

## 📊 Padrões de Código

### Injeção de Dependências

Organização modular com extension methods:

```csharp
// Api/DependencyInjection.cs
services.AddApi(configuration);

// Core/DependencyInjection.cs
services.AddCore();

// Infrastructure/DependencyInjection.cs
services.AddInfrastructure();
```

### Result Pattern

Tratamento de erros consistente:

```csharp
public record ResultStruct<T>(
    bool Success,
    T? Data,
    ErrorDetail? Error
);
```

### Repository Pattern

Abstração da camada de dados:

```csharp
public interface IClerkWebhookRepository
{
    Task<bool> ProcessUserCreatedAsync(ClerkUserData userData, ...);
    Task<bool> ProcessUserUpdatedAsync(ClerkUserData userData, ...);
    Task<bool> ProcessUserDeletedAsync(ClerkDeletedUserData userData, ...);
}
```

## 🔒 Segurança

- Verificação de assinatura Svix em todos os webhooks
- Validação de headers obrigatórios
- CORS configurável por ambiente
- Secrets gerenciados via configuração
- Logging de tentativas de acesso inválidas

## 📝 Logging

Logs estruturados com contexto rico:

```
[10:30:45 INF (1a2b:3c4d)] Processando evento webhook: user.created
[10:30:45 INF (1a2b:3c4d)] Webhook verificado com sucesso para o ApplicationId: comgas
[10:30:45 INF (1a2b:3c4d)] Persistência do usuário ClerkId user_2abc123def concluída com sucesso
```

## 🚦 Status do Projeto

✅ Integração com Clerk via webhooks  
✅ Suporte multi-tenant  
✅ Sincronização de usuários em tempo real  
✅ Logging estruturado  
✅ Documentação OpenAPI/Scalar  
✅ Arquitetura limpa e escalável  

---

## 📞 Contato

Para mais informações sobre este projeto, entre em contato:

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/lucianorodriguess/)

---
