# OrganizedScannAPI

API RESTful (.NET) com 3 entidades principais (**Motorcycles**, **Portals**, **Users**) implementando **CRUD**, **Paginação**, **HATEOAS**, **Swagger/OpenAPI**, **validações**, **Health Checks**, **JWT Authentication**, **API Versioning** e **ML.NET**. Arquitetura em camadas inspirada no projeto **SendNotification** (separação de responsabilidades, DI, testes, documentação).

## ✨ Novos Recursos Implementados

✅ **Health Checks** - Endpoints para monitoramento de saúde da API  
✅ **Versionamento de API** - Suporte a múltiplas versões da API (v1)  
✅ **Segurança JWT** - Autenticação e autorização usando JWT Bearer Tokens  
✅ **ML.NET** - Predições de tempo de manutenção usando Machine Learning  
✅ **Testes Unitários** - Cobertura de testes com xUnit  
✅ **Swagger Aprimorado** - Documentação completa com exemplos e autenticação

# 👥 Intregantes do Grupo

| Nome | RM |
|-------|----|
| Leonardo da Silva Pereira | 557598 |
| Bruno da Silva Souza | 94346 |
| Julio Samuel de Oliveira | 557453 |

---

## 🧪 Como Executar os Testes

### Executar Todos os Testes
```bash
dotnet test
```

### Executar por Classe
```bash
# AuthTests (4 testes)
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.AuthTests"

# MotorcycleServiceTests (1 teste)
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.MotorcycleServiceTests"
```

### Executar por Método Específico (nomes exatos descobertos)
```bash
# AuthTests
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.AuthTests.Register_ValidUser_Should_Return_Created"
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.AuthTests.Register_DuplicateEmail_Should_Return_BadRequest"
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.AuthTests.Login_ValidCredentials_Should_Return_Token"
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.AuthTests.Login_InvalidCredentials_Should_Return_Unauthorized"

# MotorcycleServiceTests
dotnet test --filter "FullyQualifiedName~OrganizedScannAPI.Tests.MotorcycleServiceTests.GetPagedAsync_Should_Return_Paginated_List"
```

### Descobrir Nomes Exatos dos Testes
```bash
dotnet test --list-tests -v n
```

### Rodar em modo watch (dev rápido)
```bash
dotnet watch test --project tests/OrganizedScannAPI.Tests
```

### Cobertura (opcional)
> Requer `coverlet.collector` como PackageReference no projeto de testes.
```bash
dotnet test tests/OrganizedScannAPI.Tests \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=lcov \
  /p:CoverletOutput=./TestResults/coverage
```


## 🔐 Autenticação JWT

### 1. Registrar um Usuário
```bash
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "SenhaSegura123",
  "role": 0
}
```

### 2. Fazer Login e Obter Token
```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "SenhaSegura123"
}
```

### 3. Usar o Token nas Requisições
```bash
GET /api/v1/motorcycles
Authorization: Bearer {seu_token_jwt}
```

**Nota:** Endpoints protegidos com `[Authorize]` requerem autenticação JWT válida.

---

## 🏥 Health Checks

### Endpoint Principal
```bash
GET /health
```

### Endpoints Específicos
```bash
GET /health/live   # Liveness check
GET /health/ready  # Readiness check
GET /api/v1/health # Health check com detalhes da API
```

---

## 🤖 ML.NET - Predições de Manutenção

### Predizer Tempo de Manutenção
```bash
POST /api/v1/predictions/maintenance-time
Authorization: Bearer {token}
Content-Type: application/json

{
  "year": 2022,
  "brand": "Honda"
}
```

### Analisar Padrões de Manutenção
```bash
POST /api/v1/predictions/maintenance-patterns
Authorization: Bearer {token}
```

**Nota:** Endpoints de ML.NET usam algoritmos de Machine Learning para análise de dados históricos de manutenção.

---

## 🧭 Justificativa do Domínio

O domínio modelado foca no **controle operacional de motocicletas** que passam por **portais** (checkpoints/estações de manutenção/triagem) e são manipuladas por **usuários** com diferentes perfis. As 3 entidades escolhidas representam o **núcleo mínimo do negócio** e mapeiam processos reais:

- **Motorcycles**: o *ativo* rastreado (placa/RFID, descrição de problema, previsão de disponibilidade, marca/ano). É a unidade central de valor do domínio.
- **Portals**: pontos do fluxo (ex.: *QUICK_MAINTENANCE*, *POLICE_REPORT*). Modelam os diferentes **estágios/estações** pelos quais a moto pode passar (triagem, manutenção rápida, recuperada etc.). Um *Portal* agrega regras/SLAs por tipo.
- **Users**: atores do sistema (*ADMIN*, *SUPERVISOR*, *OPERATOR*, *USER*) que **registram, atualizam e auditam** as operações, garantindo trilha e governança.

**Relações e cenários-chave**  
- Uma **Motorcycle** pertence a um **Portal** atual (`PortalId`), refletindo o estágio em que se encontra.  
- **Users** executam operações de CRUD e movimentam o fluxo.  
- Ponto de valor: acompanhar disponibilidade, priorizar manutenção e consolidar indicadores por *Portal/Tipo*.  

Esse recorte permite **operar o fluxo ponta-a-ponta** (cadastrar/atualizar/remover motos, gerenciar portais, governar acesso), mantendo **simplicidade** e **aderência** aos requisitos de REST, HATEOAS e paginação. É um domínio coeso que pode ser **expandido** (logs de movimentação, ordens de serviço, peças, auditoria, permissões avançadas) sem quebrar a base.

---

## 🧠 Justificativa da Arquitetura

A solução segue uma **arquitetura em camadas**, priorizando coesão, baixo acoplamento e testabilidade:

- **Domain** (`OrganizedScannAPI.Domain`): Entidades e enums do negócio (ex.: `Motorcycle`, `Portal`, `User`, `PortalType`, `UserRole`). Sem dependência de infraestrutura.
- **Application** (`OrganizedScannAPI.Application`): Regras de aplicação, **use cases/services** (ex.: `MotorcycleService`), **validações com FluentValidation**, **Paginação** (`PaginatedRequest/Response`) e **HATEOAS** (objetos de link e envelope). Sem conhecer detalhes de persistência.
- **Infrastructure** (`OrganizedScannAPI.Infrastructure`): Implementações de acesso a dados (EF Core/Oracle), `ApplicationDbContext`, migrations e configurações.
- **API** (`OrganizedScannAPI.Api`): Endpoints REST (Controllers), **Swagger** e pipeline (CORS, middleware de exceção etc.).

**Boas práticas aplicadas:**
- **REST com HATEOAS**: respostas incluem links (`self`, `create`, `update`, `delete`).
- **Paginação** consistente (`pageNumber`, `pageSize`) com normalização e limite máximo.
- **Validações** com FluentValidation (regras declarativas e testáveis).
- **Swagger/OpenAPI** com exemplos e modelos de dados.
- **DI (Injeção de Dependências)** e separação clara de responsabilidades.

---

## 🧰 Requisitos

- .NET SDK **8.0+**
- (Opcional) EF Core CLI: `dotnet tool install --global dotnet-ef`
- Banco de dados conforme `appsettings*.json` (por padrão Oracle/EF na **Infrastructure**)

---

## 🚀 Execução

Na raiz do repositório:

```bash
dotnet restore
dotnet build
dotnet run --project src/OrganizedScannAPI.Api/OrganizedScannAPI.Api.csproj
```

Primeira vez no HTTPS local (Windows/macOS):
```bash
dotnet dev-certs https --trust
```

## 🔌 Acesso ao Swagger
- Suba a API: `dotnet run`
- Acesse: **https://localhost:63635/swagger**  
  (use a porta exibida no console)
- O Swagger já usa endpoint relativo e CORS liberado para testes.

> Se aparecer *Failed to fetch*: use **HTTPS** da mesma porta do Swagger; mantenha `SwaggerEndpoint` relativo.

---

## 🧱 Pré‑requisitos de BD (Oracle)
- Tabelas em UPPERCASE: `PORTALS`, `USERS`, `MOTORCYCLES` (com IDENTITY).
- Mapeamento no `ApplicationDbContext` usando `ToTable("PORTALS")` etc. e `HasColumnName("ID"|"NAME"|"TYPE"|...)` para cada coluna.
- O `portalId` usado em **Motorcycles** deve existir em `PORTALS`.

---

## 🌉 PORTALS

**Nota:** Todos os endpoints estão na versão v1 da API. Use `/api/v1/...`

### POST /api/v1/portals
```json
{
  "name": "Entrada A",
  "type": 1
}
```
**Esperado:** `201 Created` com `Location: /api/portals/{id}`.

### GET /api/v1/portals
Sem parâmetros. **Esperado:** `200 OK` + lista.

### GET /api/v1/portals/{id}
Informe o `id` retornado no POST. **Esperado:** `200 OK`.

### PUT /api/v1/portals/{id}
```json
{
  "id": 1,
  "name": "Entrada A - Revisada",
  "type": 1
}
```
**Esperado:** `200 OK` ou `204 No Content`.

### DELETE /api/v1/portals/{id}
**Esperado:** `204 No Content`.  
Se houver motos vinculadas e a FK estiver `ON DELETE SET NULL`, o `PORTALID` das motos vira `NULL`.

**Casos negativos úteis**
- `type` fora de {1,2} → `400 Bad Request` (CHECK).
- `GET`/`PUT`/`DELETE` com id inexistente → `404 Not Found`.

---

## 👤 USERS

### POST /api/v1/users
```json
{
  "email": "joao.silva@example.com",
  "password": "SenhaSegura123",
  "role": 1
}
```
**Esperado:** `201 Created`.

### POST /api/v1/users (email duplicado)
Mesmo corpo acima. **Esperado:** `400 Bad Request` (unique de EMAIL).

### GET /api/v1/users
Sem parâmetros. **Esperado:** `200 OK`.

---

## 🏍️ MOTORCYCLES
> Crie antes 1 portal (ex.: `id = 2`).

### POST /api/v1/motorcycles (exemplo feliz)
```json
{
  "licensePlate": "ABC1D23",
  "rfid": "RFID-0001",
  "problemDescription": "Troca de óleo e revisão de freios",
  "portalId": 2,
  "entryDate": "2025-09-30T12:00:00Z",
  "availabilityForecast": "2025-10-02T12:00:00Z",
  "brand": "Honda",
  "year": 2022
}
```
**Esperado:** `201 Created` + `Location` do recurso.

### GET /api/v1/motorcycles (filtros e paginação)
Preencha os **query params** conforme expostos no Swagger:
- `pageNumber=1`  
- `pageSize=10`  
- `brand=Honda`  
- `year=2022`  

**Exemplo de URL:**
```
/api/v1/motorcycles?pageNumber=1&pageSize=10&brand=Honda&year=2022
```
**Esperado:** `200 OK` com `data[]`, `meta` (paginação) e `links` (HATEOAS).

### GET /api/v1/motorcycles/{id}
**Esperado:** `200 OK`.

### PUT /api/v1/motorcycles/{id}
```json
{
  "id": 1,
  "licensePlate": "ABC1D23",
  "rfid": "RFID-0001",
  "problemDescription": "Serviço concluído",
  "portalId": 2,
  "entryDate": "2025-09-30T12:00:00Z",
  "availabilityForecast": "2025-10-02T12:00:00Z",
  "brand": "Honda",
  "year": 2022
}
```
**Esperado:** `200 OK` ou `204 No Content`.

### DELETE /api/v1/motorcycles/{id}
**Esperado:** `204 No Content`.

**Casos negativos úteis**
- `portalId` inexistente → `400` (FK `PORTALID`).
- Placa (`LICENSEPLATE`) duplicada → `400` (unique).
- RFID duplicado → `400` (unique).
- `year` fora do range (ex.: 1800) → `400` (CHECK).
- JSON malformado (aspas/vírgulas) → `400` + binding error (*The motorcycle field is required*).

---

## 🧪 Lote de exemplos de motos
Use `portalId` existente (2/3). Todas com placa/RFID únicos.

```json
{
  "licensePlate": "ABC2E34",
  "rfid": "RFID-0002",
  "problemDescription": "Revisão de 10.000 km",
  "portalId": 2,
  "entryDate": "2025-10-01T09:00:00Z",
  "availabilityForecast": "2025-10-03T14:00:00Z",
  "brand": "Yamaha",
  "year": 2021
}
```
```json
{
  "licensePlate": "BCD3F45",
  "rfid": "RFID-0003",
  "problemDescription": "Troca de pastilhas de freio",
  "portalId": 2,
  "entryDate": "2025-10-01T10:30:00Z",
  "availabilityForecast": "2025-10-02T16:00:00Z",
  "brand": "Honda",
  "year": 2020
}
```
```json
{
  "licensePlate": "CDE4G56",
  "rfid": "RFID-0004",
  "problemDescription": "Alinhamento e balanceamento",
  "portalId": 3,
  "entryDate": "2025-10-01T13:15:00Z",
  "availabilityForecast": "2025-10-04T11:00:00Z",
  "brand": "Suzuki",
  "year": 2019
}
```
```json
{
  "licensePlate": "DEF5H67",
  "rfid": "RFID-0005",
  "problemDescription": "Vazamento de óleo no cárter",
  "portalId": 2,
  "entryDate": "2025-10-01T15:45:00Z",
  "availabilityForecast": "2025-10-05T10:00:00Z",
  "brand": "Kawasaki",
  "year": 2022
}
```
```json
{
  "licensePlate": "EFG6I78",
  "rfid": "RFID-0006",
  "problemDescription": "Substituição de corrente e coroa",
  "portalId": 2,
  "entryDate": "2025-10-02T08:20:00Z",
  "availabilityForecast": "2025-10-03T18:00:00Z",
  "brand": "BMW",
  "year": 2023
}
```
```json
{
  "licensePlate": "FGH7J89",
  "rfid": "RFID-0007",
  "problemDescription": "Pane elétrica intermitente",
  "portalId": 3,
  "entryDate": "2025-10-02T09:10:00Z",
  "availabilityForecast": "2025-10-06T09:00:00Z",
  "brand": "Triumph",
  "year": 2018
}
```
```json
{
  "licensePlate": "GHI8K90",
  "rfid": "RFID-0008",
  "problemDescription": "Troca de pneu traseiro",
  "portalId": 2,
  "entryDate": "2025-10-02T11:00:00Z",
  "availabilityForecast": "2025-10-02T16:30:00Z",
  "brand": "Dafra",
  "year": 2021
}
```
```json
{
  "licensePlate": "HIJ9L01",
  "rfid": "RFID-0009",
  "problemDescription": "Reparo no sistema de injeção",
  "portalId": 2,
  "entryDate": "2025-10-02T13:40:00Z",
  "availabilityForecast": "2025-10-04T17:00:00Z",
  "brand": "Honda",
  "year": 2017
}
```
```json
{
  "licensePlate": "IJK0M12",
  "rfid": "RFID-0010",
  "problemDescription": "Ruído na suspensão dianteira",
  "portalId": 3,
  "entryDate": "2025-10-02T14:25:00Z",
  "availabilityForecast": "2025-10-05T12:00:00Z",
  "brand": "Yamaha",
  "year": 2022
}
```
```json
{
  "licensePlate": "JKL1N23",
  "rfid": "RFID-0011",
  "problemDescription": "Substituição de bateria",
  "portalId": 2,
  "entryDate": "2025-10-03T08:05:00Z",
  "availabilityForecast": "2025-10-03T12:00:00Z",
  "brand": "Suzuki",
  "year": 2020
}
```
```json
{
  "licensePlate": "KLM2O34",
  "rfid": "RFID-0012",
  "problemDescription": "Falha no sensor de velocidade",
  "portalId": 2,
  "entryDate": "2025-10-03T09:55:00Z",
  "availabilityForecast": "2025-10-06T15:00:00Z",
  "brand": "Honda",
  "year": 2024
}
```
```json
{
  "licensePlate": "LMN3P45",
  "rfid": "RFID-0013",
  "problemDescription": "Troca de embreagem",
  "portalId": 3,
  "entryDate": "2025-10-03T11:30:00Z",
  "availabilityForecast": "2025-10-07T10:00:00Z",
  "brand": "Kawasaki",
  "year": 2019
}
```

---

## 🧭 Status Codes esperados
- `201 Created` em POST, com `Location` do recurso.
- `200 OK` em GET/PUT (ou `204 No Content` em PUT/DELETE, conforme implementação).
- `400 Bad Request` em validação/unique/fk/JSON malformado.
- `404 Not Found` para ids inexistentes.

---

## 🛠️ Troubleshooting (erros comuns)
- **ORA-00942 (tabela não existe)**: nomes de **tabela** com *case* diferente; use `ToTable("MOTORCYCLES")` etc.
- **ORA-00904 (identificador inválido)**: nomes de **colunas** com *case* diferente; use `HasColumnName("ID"|...)`.
- **ORA-02291 (FK)**: `portalId` não existe.
- **Failed to fetch**: acesse o Swagger na **mesma origem/porta (HTTPS)** e mantenha `SwaggerEndpoint` relativo.

> A porta aparece no console ao iniciar (ex.: `http://localhost:63636`).

---

