# OrganizedScannAPI

API RESTful (.NET) com 3 entidades principais (**Motorcycles**, **Portals**, **Users**) implementando **CRUD**, **Paginação**, **HATEOAS**, **Swagger/OpenAPI** e **validações**. Arquitetura em camadas inspirada no projeto **SendNotification** (separação de responsabilidades, DI, testes, documentação).

# 👥 Intregantes do Grupo

| Nome | RM |
|-------|----|
| Leonardo da Silva Pereira | 557598 |
| Bruno da Silva Souza | 94346 |
| Julio Samuel de Oliveira | 557453 |

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

**Swagger UI**:
- `http://localhost:<PORT>/swagger`
- `https://localhost:<PORT>/swagger`
- - `http://localhost:63636/swagger`

> A porta aparece no console ao iniciar (ex.: `http://localhost:63636`).

---

## ⚙️ Configuração do Banco

Ajuste a **connection string** em `src/OrganizedScannAPI.Api/appsettings.Development.json` (ou `appsettings.json`).

Para criar/atualizar o banco via EF Core (se aplicável):

```bash
dotnet tool install --global dotnet-ef

# criar migration inicial
dotnet ef migrations add InitialCreate   -p src/OrganizedScannAPI.Infrastructure/OrganizedScannAPI.Infrastructure.csproj   -s src/OrganizedScannAPI.Api/OrganizedScannAPI.Api.csproj

# aplicar no banco
dotnet ef database update   -p src/OrganizedScannAPI.Infrastructure/OrganizedScannAPI.Infrastructure.csproj   -s src/OrganizedScannAPI.Api/OrganizedScannAPI.Api.csproj
```

> Em POC/local, é possível usar **InMemory** para evitar dependência de SGBD.

---

## 🔎 Exemplos de Uso (Endpoints)

> Troque `<PORT>` pela porta exibida ao rodar a API.

### Motorcycles

**Listar (paginado)**
```bash
curl "http://localhost:<PORT>/api/motorcycles?pageNumber=1&pageSize=10"
```

**Buscar por ID**
```bash
curl "http://localhost:<PORT>/api/motorcycles/1"
```

**Criar**
```bash
curl -X POST "http://localhost:<PORT>/api/motorcycles" -H "Content-Type: application/json" -d '{
  "licensePlate": "ABC1D23",
  "rfid": "RFID-0001",
  "problemDescription": "Troca de óleo",
  "portalId": 1,
  "entryDate": "2025-09-22T12:00:00Z",
  "availabilityForecast": "2025-09-24T12:00:00Z",
  "brand": "Honda",
  "year": 2022
}'
```

**Atualizar**
```bash
curl -X PUT "http://localhost:<PORT>/api/motorcycles/1" -H "Content-Type: application/json" -d '{
  "id": 1,
  "licensePlate": "ABC1D23",
  "rfid": "RFID-0001",
  "problemDescription": "Troca de óleo (atualizada)",
  "portalId": 1,
  "entryDate": "2025-09-22T12:00:00Z",
  "availabilityForecast": "2025-09-25T12:00:00Z",
  "brand": "Honda",
  "year": 2022
}'
```

**Remover**
```bash
curl -X DELETE "http://localhost:<PORT>/api/motorcycles/1"
```

### Portals

**Listar (paginado)**
```bash
curl "http://localhost:<PORT>/api/portals?pageNumber=1&pageSize=10"
```

**Buscar por ID**
```bash
curl "http://localhost:<PORT>/api/portals/1"
```

**Criar**
```bash
curl -X POST "http://localhost:<PORT>/api/portals" -H "Content-Type: application/json" -d '{
  "name": "Manutenção Rápida",
  "type": "QUICK_MAINTENANCE"
}'
```

**Atualizar**
```bash
curl -X PUT "http://localhost:<PORT>/api/portals/1" -H "Content-Type: application/json" -d '{
  "id": 1,
  "name": "Manutenção Rápida (Atualizado)",
  "type": "QUICK_MAINTENANCE"
}'
```

**Remover**
```bash
curl -X DELETE "http://localhost:<PORT>/api/portals/1"
```

### Users

**Listar (paginado)**
```bash
curl "http://localhost:<PORT>/api/users?pageNumber=1&pageSize=10"
```

**Buscar por ID**
```bash
curl "http://localhost:<PORT>/api/users/1"
```

**Criar**
```bash
curl -X POST "http://localhost:<PORT>/api/users" -H "Content-Type: application/json" -d '{
  "email": "admin@example.com",
  "password": "Secret@123",
  "role": "ADMIN"
}'
```

**Atualizar**
```bash
curl -X PUT "http://localhost:<PORT>/api/users/1" -H "Content-Type: application/json" -d '{
  "id": 1,
  "email": "admin@example.com",
  "password": "Secret@123",
  "role": "ADMIN"
}'
```

**Remover**
```bash
curl -X DELETE "http://localhost:<PORT>/api/users/1"
```

---

## 🔗 HATEOAS & Paginação

Respostas **paginadas**:
```json
{
  "data": {
    "pageNumber": 1,
    "pageSize": 10,
    "total": 42,
    "totalPages": 5,
    "items": [ /* ... */ ]
  },
  "links": [
    { "rel": "self", "href": "..." },
    { "rel": "create", "href": "...", "method": "POST" }
  ]
}
```

Respostas **por ID** incluem links para `self`, `update` e `delete`.

---

## 🧪 Testes

Para executar os testes:
```bash
dotnet test
```
Opcional (apontando o projeto de testes):
```bash
dotnet test tests/OrganizedScannAPI.Tests/OrganizedScannAPI.Tests.csproj
```

---

## 📝 Observações

- Em desenvolvimento local com HTTPS, aceite o **certificado dev**.
- Se usar Oracle/EF, valide provider, permissões e a **connection string**.
- O projeto possui **Swagger** com exemplos de payloads para facilitar a avaliação.
