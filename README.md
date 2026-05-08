# 🚀 Microservices Architecture - Projeto API

Um exemplo didático de arquitetura de microservices com os patterns **API Gateway**, **BFF (Backend for Frontend)** e **Aggregator**.

## 📋 Estrutura do Projeto

```
projeto-api/
├── src/
│   ├── ApiGateway/
│   ├── Bff/
│   ├── Aggregator/
│   └── Services/
│       ├── UserService/
│       ├── OrderService/
│       └── ProductService/
```

## 🚀 Como Executar

### ✅ **Opção 1: Windows Batch**

```bash
cd C:\Users\narri\repositorios\projeto-api
run-all.bat
```

Ou simplesmente **duplo clique** no arquivo `run-all.bat` no explorador de arquivos

---

### ✅ **Opção 2: Manual (Terminal por Serviço)**

Abra 6 terminais/janelas PowerShell:

```powershell
# Terminal 1 - UserService
cd C:\Users\narri\repositorios\projeto-api\src\Services\UserService
dotnet run

# Terminal 2 - OrderService
cd C:\Users\narri\repositorios\projeto-api\src\Services\OrderService
dotnet run

# Terminal 3 - ProductService
cd C:\Users\narri\repositorios\projeto-api\src\Services\ProductService
dotnet run

# Terminal 4 - Aggregator
cd C:\Users\narri\repositorios\projeto-api\src\Aggregator
dotnet run

# Terminal 5 - Bff
cd C:\Users\narri\repositorios\projeto-api\src\Bff
dotnet run

# Terminal 6 - ApiGateway
cd C:\Users\narri\repositorios\projeto-api\src\ApiGateway
dotnet run
```

---

## 🎯 Responsabilidades de Cada Projeto

### **ApiGateway**
- **Padrão**: API Gateway
- **Responsabilidade**: Ponto de entrada único da aplicação
- **Função**:
  - Expõe endpoints que chamam o BFF via HttpClient
  - Isola os serviços internos do cliente externo
  - Fornece endpoints de management (health, info, status)
- **Tecnologias**: ASP.NET Core, HttpClient, Swagger

### **Bff**
- **Padrão**: Backend for Frontend (BFF)
- **Responsabilidade**: Camada de adaptação para o frontend
- **Função**:
  - Recebe requisições do ApiGateway
  - Orquestra chamadas ao Aggregator
  - Transforma os dados agregados em formato específico para a UI
  - Exemplo: converte dados brutos em dashboard formatado com totalOrders, lastOrders, highlights
- **Tecnologias**: ASP.NET Core, HttpClient, Swashbuckle

### **Aggregator**
- **Padrão**: Aggregator Service
- **Responsabilidade**: Agregar dados de múltiplos microserviços
- **Função**:
  - Recebe requisições do Bff
  - Faz chamadas paralelas aos microserviços (UserService, OrderService, ProductService)
  - Compõe todas as respostas em um único objeto JSON
  - Reduz o número de chamadas HTTP do frontend para uma única requisição
- **Tecnologias**: ASP.NET Core, HttpClient, Task.WhenAll

### **UserService**
- **Padrão**: Microservice
- **Responsabilidade**: Gerenciar dados de usuários
- **Função**:
  - Endpoint: `GET /users/{id}` - retorna dados de um usuário específico
  - Mantém mock data de usuários (id, name, email)
  - Responsabilidade única: apenas dados de usuário
- **Tecnologias**: ASP.NET Core, Minimal APIs

### **OrderService**
- **Padrão**: Microservice
- **Responsabilidade**: Gerenciar pedidos
- **Função**:
  - Endpoint: `GET /orders?userId={id}` - retorna pedidos de um usuário
  - Endpoint: `GET /orders` - retorna todos os pedidos
  - Mantém mock data de pedidos (id, userId, productId, total)
  - Responsabilidade única: apenas dados de pedidos
- **Tecnologias**: ASP.NET Core, Minimal APIs

### **ProductService**
- **Padrão**: Microservice
- **Responsabilidade**: Gerenciar produtos
- **Função**:
  - Endpoint: `GET /products?ids={ids}` - retorna produtos específicos (ex: 10,11,12)
  - Endpoint: `GET /products` - retorna todos os produtos
  - Mantém mock data de produtos (id, name, price)
  - Responsabilidade única: apenas dados de produtos
- **Tecnologias**: ASP.NET Core, Minimal APIs

## 🔄 Diagrama de Relacionamentos

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENTE HTTP                            │
│                    (Browser, Mobile App)                        │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ GET http://localhost:5000/dashboard/web
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        API GATEWAY                              │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Endpoints Explícitos                                    │   │
│  │  • GET /dashboard/web → HttpClient → BFF                 │   │
│  │  • GET /dashboard/mobile → HttpClient → BFF              │   │
│  │  • GET /health, /gateway/info, /gateway/status           │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ Chamada HttpClient explícita
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                            BFF                                  │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Backend for Frontend                                   │    │
│  │  • Transforma dados para UI                             │    │
│  │  • Chama Aggregator                                     │    │
│  └─────────────────────────────────────────────────────────┘    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ GET http://localhost:5002/dashboard-data
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                            AGGREGATOR                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Aggregator Service                                     │    │
│  │  • Task.WhenAll (chamadas paralelas)                    │    │
│  │  • Compõe respostas em um único objeto                  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────┬───────────────────────────────┘
                                  ▼     
                                        
     ┌───────▼───────┐        ┌───────▼───────┐        ┌───────▼───────┐
     │    PARALELO   │        │    PARALELO   │        │    PARALELO   │
     └───────┬───────┘        └───────┬───────┘        └───────┬───────┘
             │                        │                        │
             │                        │                        │
┌────────────▼───────────┐  ┌─────────▼───────────┐  ┌─────────▼───────────┐
│      USER SERVICE      │  │     ORDER SERVICE   │  │   PRODUCT SERVICE   │
│  • /users/{id}         │  │  • /orders?userId=  │  │ • /products?ids=    │
│  • Dados de usuário    │  │  • Dados de pedidos │  │ • Dados de produtos │
└────────────────────────┘  └─────────────────────┘  └─────────────────────┘
```

### Legenda do Fluxo:

1. **Cliente** → **ApiGateway**: Requisição para endpoint explícito (ex: /dashboard/web)
2. **ApiGateway** → **Bff**: Chamada HttpClient explícita (cliente não sabe do BFF)
3. **Bff** → **Aggregator**: Solicita dados agregados
4. **Aggregator** → **Microserviços**: Chamadas **PARALELAS** simultâneas
5. **Microserviços** → **Aggregator**: Cada um retorna seus dados
6. **Aggregator** → **Bff**: Retorna objeto composto {user, orders, products}
7. **Bff** → **ApiGateway**: Transforma em formato de dashboard {name, email, totalOrders, ...}
8. **ApiGateway** → **Cliente**: Resposta final formatada

## 📊 Fluxo de Requisição

```
Cliente HTTP
    ↓
Requisição: GET http://localhost:5000/dashboard/web
    ↓
ApiGateway (HttpClient) - Porta 5000
    ↓ (Chama BFF explicitamente)
Bff - Porta 5001
    ↓
Requisição: GET http://localhost:5002/dashboard-data
    ↓
Aggregator - Porta 5002
    ↓
Chama em PARALELO (Task.WhenAll):
├── GET http://localhost:5003/users/1 (UserService)
├── GET http://localhost:5004/orders?userId=1 (OrderService)
└── GET http://localhost:5005/products?ids=10,11,12 (ProductService)
    ↓
Retorna dados agregados para Bff
    ↓
Bff transforma em:
{
  "name": "João Silva",
  "email": "joao@example.com",
  "totalOrders": 2,
  "lastOrders": [...],
  "highlights": [...]
}
    ↓
Retorna ao cliente via ApiGateway
```

---

## 🌐 URLs para Testar

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Gateway** | http://localhost:5000 | Swagger (endpoints explícitos) |
| **API Gateway** | http://localhost:5000/dashboard/web | Dashboard Web |
| **API Gateway** | http://localhost:5000/dashboard/mobile | Dashboard Mobile |
| **BFF** | http://localhost:5001 | Swagger |
| **Aggregator** | http://localhost:5002/dashboard-data | Dados agregados |
| **UserService** | http://localhost:5003 | Swagger |
| **OrderService** | http://localhost:5004 | Swagger |
| **ProductService** | http://localhost:5005 | Swagger |

---

## 🛠️ Tecnologias Utilizadas

- **ASP.NET Core 8** (net8.0)
- **Minimal APIs** (sem Controllers)
- **Swagger/Swashbuckle** (documentação automática)
- **HttpClient** (comunicação entre serviços)
- **Mock Data** (sem banco de dados)
- **Task.WhenAll** (paralelismo)

---

## 📚 Padrões Demonstrados

### 1. **API Gateway**
- Single ponto de entrada
- Endpoints explícitos que chamam BFF via HttpClient
- Endpoints de management (health, info, status)

### 2. **BFF - Backend for Frontend**
- Camada intermediária
- Adaptação de respostas para frontend
- Orquestra chamadas internas

### 3. **Aggregator**
- Agrega dados de múltiplos serviços
- Paralelismo com Task.WhenAll
- Composição de respostas

### 4. **Microservices** (User, Order, Product)
- Serviços independentes
- Responsabilidade única
- 