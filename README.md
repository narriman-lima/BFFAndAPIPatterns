# 🚀 Microservices Architecture - Projeto API

Um exemplo didático de arquitetura de microservices com os patterns **API Gateway**, **BFF (Backend for Frontend)** e **Aggregator**.

## 📋 Estrutura do Projeto

```
projeto-api/
├── src/
│   ├── ApiGateway/              (Porta 5000)
│   ├── Bff.Web/                 (Porta 5001)
│   ├── Aggregator/              (Porta 5002)
│   └── Services/
│       ├── UserService/         (Porta 5003)
│       ├── OrderService/        (Porta 5004)
│       └── ProductService/      (Porta 5005)
└── run-all.bat                  (Script Batch)
```

## 🎯 Responsabilidades de Cada Projeto

### **ApiGateway** (Porta 5000)
- **Padrão**: API Gateway
- **Responsabilidade**: Ponto de entrada único da aplicação
- **Função**:
  - Atua como reverse proxy usando YARP (Yet Another Reverse Proxy)
  - Roteia todas as requisições para o BFF.Web
  - Isola os serviços internos do cliente externo
  - Futuramente poderia adicionar: autenticação, rate limiting, CORS, logging centralizado
- **Tecnologias**: YARP, ASP.NET Core

### **Bff.Web** (Porta 5001)
- **Padrão**: Backend for Frontend (BFF)
- **Responsabilidade**: Camada de adaptação para o frontend
- **Função**:
  - Recebe requisições do ApiGateway
  - Orquestra chamadas ao Aggregator
  - Transforma os dados agregados em formato específico para a UI
  - Exemplo: converte dados brutos em dashboard formatado com totalOrders, lastOrders, highlights
- **Tecnologias**: ASP.NET Core, HttpClient, Swashbuckle

### **Aggregator** (Porta 5002)
- **Padrão**: Aggregator Service
- **Responsabilidade**: Agregar dados de múltiplos microserviços
- **Função**:
  - Recebe requisições do BFF.Web
  - Faz chamadas paralelas aos microserviços (UserService, OrderService, ProductService)
  - Usa `Task.WhenAll` para performance otimizada
  - Compõe todas as respostas em um único objeto JSON
  - Reduz o número de chamadas HTTP do frontend para uma única requisição
- **Tecnologias**: ASP.NET Core, HttpClient, Task.WhenAll

### **UserService** (Porta 5003)
- **Padrão**: Microservice
- **Responsabilidade**: Gerenciar dados de usuários
- **Função**:
  - Endpoint: `GET /users/{id}` - retorna dados de um usuário específico
  - Mantém mock data de usuários (id, name, email)
  - Responsabilidade única: apenas dados de usuário
- **Tecnologias**: ASP.NET Core, Minimal APIs

### **OrderService** (Porta 5004)
- **Padrão**: Microservice
- **Responsabilidade**: Gerenciar pedidos
- **Função**:
  - Endpoint: `GET /orders?userId={id}` - retorna pedidos de um usuário
  - Endpoint: `GET /orders` - retorna todos os pedidos
  - Mantém mock data de pedidos (id, userId, productId, total)
  - Responsabilidade única: apenas dados de pedidos
- **Tecnologias**: ASP.NET Core, Minimal APIs

### **ProductService** (Porta 5005)
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
│                         CLIENTE HTTP                             │
│                    (Browser, Mobile App)                        │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ GET http://localhost:5000/dashboard
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API GATEWAY (5000)                         │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  YARP - Reverse Proxy                                  │   │
│  │  • Roteamento: {**catch-all} → bff-cluster             │   │
│  │  • Destino: http://localhost:5001 (Bff.Web)           │   │
│  └─────────────────────────────────────────────────────────┘   │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ Proxy transparente
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                       BFF.WEB (5001)                            │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Backend for Frontend                                   │   │
│  │  • Transforma dados para UI                             │   │
│  │  • Chama Aggregator                                     │   │
│  └─────────────────────────────────────────────────────────┘   │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ GET http://localhost:5002/dashboard-data
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                     AGGREGATOR (5002)                           │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Aggregator Service                                    │   │
│  │  • Task.WhenAll (chamadas paralelas)                   │   │
│  │  • Compõe respostas em um único objeto                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└────────────┬────────────────────────┬───────────────────────────┘
             │                        │
             │                        │
     ┌───────▼────────┐      ┌───────▼──────────┐
     │   PARALELO     │      │    PARALELO      │
     └───────┬────────┘      └───────┬──────────┘
             │                        │
             │                        │
┌────────────▼─────────┐  ┌──────────▼──────────┐  ┌──────────▼──────────┐
│  USER SERVICE (5003) │  │ ORDER SERVICE (5004)│  │PRODUCT SERVICE(5005)│
│  • /users/{id}       │  │  • /orders?userId=  │  │ • /products?ids=    │
│  • Dados de usuário  │  │  • Dados de pedidos │  │ • Dados de produtos │
└──────────────────────┘  └─────────────────────┘  └─────────────────────┘
```

### Legenda do Fluxo:

1. **Cliente** → **ApiGateway**: Requisição inicial para o endpoint principal
2. **ApiGateway** → **Bff.Web**: Proxy transparente (cliente não sabe do BFF)
3. **Bff.Web** → **Aggregator**: Solicita dados agregados
4. **Aggregator** → **Microserviços**: Chamadas **PARALELAS** simultâneas
5. **Microserviços** → **Aggregator**: Cada um retorna seus dados
6. **Aggregator** → **Bff.Web**: Retorna objeto composto {user, orders, products}
7. **Bff.Web** → **ApiGateway**: Transforma em formato de dashboard {name, email, totalOrders, ...}
8. **ApiGateway** → **Cliente**: Resposta final formatada

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

# Terminal 5 - Bff.Web
cd C:\Users\narri\repositorios\projeto-api\src\Bff.Web
dotnet run

# Terminal 6 - ApiGateway
cd C:\Users\narri\repositorios\projeto-api\src\ApiGateway
dotnet run
```

---

## 🌐 URLs para Testar

Uma vez que todos os serviços estão rodando:

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **API Gateway** | http://localhost:5000/dashboard | Endpoint principal (YARP Proxy) |
| **BFF.Web** | http://localhost:5001/dashboard | Dashboard em BFF |
| **BFF.Web Swagger** | http://localhost:5001/swagger | API Docs |
| **Aggregator** | http://localhost:5002/dashboard-data | Agregador de dados |
| **Aggregator Swagger** | http://localhost:5002/swagger | API Docs |
| **UserService** | http://localhost:5003/users/1 | Buscar usuário |
| **UserService Swagger** | http://localhost:5003/swagger | API Docs |
| **OrderService** | http://localhost:5004/orders?userId=1 | Buscar pedidos |
| **OrderService Swagger** | http://localhost:5004/swagger | API Docs |
| **ProductService** | http://localhost:5005/products?ids=10,11,12 | Buscar produtos |
| **ProductService Swagger** | http://localhost:5005/swagger | API Docs |

---

## 📊 Fluxo de Requisição

```
Cliente HTTP
    ↓
Requisição: GET http://localhost:5000/dashboard
    ↓
ApiGateway (YARP) - Porta 5000
    ↓ (Reverse Proxy)
Bff.Web - Porta 5001
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
Retorna dados agregados para Bff.Web
    ↓
Bff.Web transforma em:
{
  "name": "João Silva",
  "email": "joao@example.com",
  "totalOrders": 2,
  "lastOrders": [...],
  "highlights": [...]
}
    ↓
Retorna ao cliente
```

---

## 📝 Endpoints Disponíveis

### UserService (5003)
```http
GET /users/1
```
**Resposta:**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao@example.com"
}
```

### OrderService (5004)
```http
GET /orders?userId=1
```
**Resposta:**
```json
[
  {
    "id": 1,
    "userId": 1,
    "productId": 10,
    "total": 150.50
  }
]
```

### ProductService (5005)
```http
GET /products?ids=10,11,12
```
**Resposta:**
```json
[
  {
    "id": 10,
    "name": "Laptop",
    "price": 1500.00
  }
]
```

### Aggregator (5002)
```http
GET /dashboard-data
```
**Resposta:**
```json
{
  "user": {/* UserService Response */},
  "orders": [/* OrderService Response */],
  "products": [/* ProductService Response */]
}
```

### BFF.Web (5001)
```http
GET /dashboard
```
**Resposta:**
```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "totalOrders": 2,
  "lastOrders": [...],
  "highlights": [...]
}
```

### ApiGateway (5000)
```http
GET /dashboard
```
Roteia para Bff.Web através de reverse proxy YARP

---

## 🛠️ Tecnologias Utilizadas

- **ASP.NET Core 10** (net10.0)
- **Minimal APIs** (sem Controllers)
- **Swagger/Swashbuckle** (documentação automática)
- **YARP** (Reverse Proxy - API Gateway)
- **HttpClient** (comunicação entre serviços)
- **Mock Data** (sem banco de dados)
- **Task.WhenAll** (paralelismo)

---

## 📚 Padrões Demonstrados

### 1. **API Gateway** (ApiGateway na porta 5000)
- Singleponto de entrada
- Reverse proxy com YARP
- Roteamento para BFF

### 2. **BFF - Backend for Frontend** (Bff.Web na porta 5001)
- Camada intermediária
- Adaptação de respostas para frontend
- Orquestra chamadas internas

### 3. **Aggregator** (Aggregator na porta 5002)
- Agrega dados de múltiplos serviços
- Paralelismo com Task.WhenAll
- Composição de respostas

### 4. **Microservices** (User, Order, Product)
- Serviços independentes
- Responsabilidade única
- Escaláveis individualmente

---

## 🎓 Para Fins Didáticos

Este projeto foi criado para demonstrar:
- Comunicação entre microservices
- Padrão API Gateway
- Padrão BFF
- Padrão Aggregator
