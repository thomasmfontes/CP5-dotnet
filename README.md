# CP5 - API de Filmes e Avaliações
> Advanced Business Development with .NET - FIAP 2026

## Integrantes do Grupo

- Thomas Fontes - RM: 562254
- Matheus Molina - RM: 563399
- Gabriel Maciel - RM: 562795
- Vitória Rodrigues - RM: 565160
- Augusto Bonomo - RM: 565155

**Vídeo de Demonstração:** https://youtu.be/ov_u1noACCc

---

## 1. Visão Geral do Projeto

Esta API RESTful foi desenvolvida em **.NET 8** utilizando **Controllers**, seguindo os princípios de **Clean Architecture**, Repository Pattern, DTOs e extensões de mapeamento.

O projeto é a evolução da API de Filmes e Avaliações (desenvolvida no CP4), adicionando novos requisitos solicitados no CP5:
- Adoção do padrão **Operation Result** em todas as camadas de negócio e apresentação.
- Abordagem dupla de **Erros Customizados** no repositório: retorno de erro via `OperationResult` e lançamento de exceções específicas (`EntityNotFoundException`, `RepositoryOperationException`).
- Paginação estruturada com metadados completos utilizando **PageResult**.
- Enriquecimento de hipermídia com **HATEOAS** (`_links`) em todos os recursos.
- Consumo de **Serviço Externo** (API pública do TVMaze) com resiliência via **Polly** (Retry exponencial, Circuit Breaker e Fallback).
- Controle de **Autenticação e Autorização com JWT Bearer**, separando perfis de Administrador (`Admin`) e Usuário comum (`User`).
- **Observabilidade**: logging com Serilog em arquivo rotativo diário e **Health Checks** segregados por tags (`live` para liveness probe e `ready` para readiness probe).

---

## 2. Arquitetura da Solução

A solução foi estruturada em 6 projetos independentes respeitando a regra de dependência da Clean Architecture:

```
CP5/
├── CP5.Domain/               # Camada de Domínio (Entidades, Interfaces e Exceções)
│   ├── Entities/             # Filme.cs, Avaliacao.cs, User.cs
│   ├── Common/               # OperationResult.cs, PageResult.cs, LinkDto.cs, ExternalMovieDetails.cs
│   ├── Exceptions/           # DomainValidationException.cs, EntityNotFoundException.cs, etc.
│   └── Interfaces/           # IFilmeRepository.cs, IAvaliacaoRepository.cs, IUserRepository.cs, IExternalMovieService.cs
│
├── CP5.Application/          # Camada de Aplicação (Casos de Uso, DTOs e Mapeamentos)
│   ├── DTOs/                 # Filmes, Avaliacoes, Auth, Common (PagedRequestDto)
│   ├── Mappers/              # FilmeMapper.cs, AvaliacaoMapper.cs
│   ├── Interfaces/           # IFilmeService.cs, IAvaliacaoService.cs, IAuthService.cs, IHateoasService.cs
│   └── Services/             # FilmeService.cs, AvaliacaoService.cs, AuthService.cs, HateoasService.cs
│
├── CP5.Infrastructure/       # Camada de Infraestrutura (Banco Oracle, EF Core, HttpClient e Segurança)
│   ├── Data/                 # AppDbContext.cs, DbInitializer.cs
│   ├── Data/Configurations/  # FilmeConfiguration.cs, AvaliacaoConfiguration.cs, UserConfiguration.cs
│   ├── Repositories/         # FilmeRepository.cs, AvaliacaoRepository.cs, UserRepository.cs
│   ├── ExternalServices/     # TvMazeExternalMovieService.cs (Polly + Fallback)
│   └── Security/             # JwtTokenGenerator.cs (HMAC-SHA256)
│
├── CP5.Api/                  # Camada de Apresentação (Web API com Controllers .NET 8)
│   ├── Controllers/          # FilmesController.cs, AvaliacoesController.cs, AuthController.cs
│   ├── Middlewares/          # GlobalExceptionMiddleware.cs
│   ├── appsettings.json      # Configurações de Conexão, JWT e Logging
│   └── Program.cs            # Pipeline HTTP, Rate Limiter, Health Checks e Swagger
│
├── CP5.UnitTests/            # Testes de Unidade (xUnit + Moq) - 30 testes
│   ├── Domain/               # Validações de entidades e regras de negócio
│   ├── Common/               # Testes de OperationResult e PageResult
│   └── Services/             # Testes de serviços de aplicação isolados
│
└── CP5.IntegrationTests/     # Testes de Integração / Funcionais (xUnit + WebApplicationFactory) - 15 testes
    ├── CustomWebApplicationFactory.cs
    ├── HealthCheckTests.cs   # Testes dos endpoints /health, /health/live e /health/ready
    ├── AuthEndpointsTests.cs # Testes de login e geração de JWT
    └── FilmesEndpointsTests.cs # Ciclo real de requisições, paginação, HATEOAS e autorização
```

---

## 3. Funcionalidades e Requisitos Atendidos

| Requisito | Implementação Técnica |
|---|---|
| **Clean Architecture** | 4 camadas segregadas (`Domain`, `Application`, `Infrastructure`, `Api`) |
| **Operation Result** | Padrão `OperationResult` e `OperationResult<T>` adotado em serviços e controllers |
| **Erros Customizados no Repo** | Retorno de erro no repositório via `OperationResult` e lançamento via `EntityNotFoundException` |
| **PageResult** | Envelope de paginação com `PageNumber`, `PageSize`, `TotalItems`, `TotalPages`, `HasPreviousPage` e `HasNextPage` |
| **HATEOAS** | Links hipermídia injetados nos recursos (`self`, `update`, `delete`, `filme-avaliacoes`, `external-info`) |
| **Serviço Externo com Resiliência** | Consumo da API do TVMaze via `HttpClientFactory` com Retry, Circuit Breaker e Fallback (Polly) |
| **Autenticação & Autorização** | JWT Bearer Token assinado, endpoints protegidos por Roles (`Admin` e `User`) e suporte no Swagger |
| **Otimização de Consultas** | `AsNoTracking()` em todas as leituras, índices no banco, Response Compression (Brotli/Gzip) e Rate Limiting |
| **Observabilidade** | Serilog com saída em console e arquivo rotativo diário (`logs/cp5-filmes-.log`) |
| **Health Checks** | `/health/live` (tag `live`), `/health/ready` (tag `ready`) e `/health` com diagnóstico completo |
| **Testes Automatizados** | **45 testes aprovados** (30 unitários + 15 de integração) no banco da aplicação |

---

## 4. Configuração do Ambiente e Banco de Dados

### 4.1 Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022 ou VS Code
- Oracle Database (ou execução com banco em memória)

### 4.2 Configuração da Conexão (`CP5.Api/appsettings.json`)
A connection string do Oracle está configurada no `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "DATA SOURCE=oracle.fiap.com.br:1521/ORCL;USER ID=rm565160;PASSWORD=030701;"
  },
  "UseInMemoryDatabase": false,
  "Jwt": {
    "SecretKey": "MinhaChaveSuperSecretaParaCP5Fiap2026_AssinaturaJWT_123456789",
    "Issuer": "CP5_FilmesApi",
    "Audience": "CP5_FilmesApi_Clients",
    "ExpirationHours": 4
  }
}
```

O script DDL com a criação de tabelas, índices e carga inicial no Oracle está disponível no arquivo [`script_oracle.sql`](script_oracle.sql).

---

## 5. Como Executar o Projeto

### Pelo Terminal:
```bash
# Restaurar dependências
dotnet restore

# Compilar a solução
dotnet build

# Executar a API
dotnet run --project CP5.Api/CP5.Api.csproj
```

A API iniciará por padrão em `http://localhost:5209`. A interface interativa do Swagger pode ser acessada na raiz:
- **Swagger UI:** `http://localhost:5209` (ou `http://localhost:5209/swagger`)

---

## 6. Como Executar os Testes Automatizados

Para executar toda a suíte de testes de unidade e integração:

```bash
dotnet test CP5_FilmesApi.sln
```

Caso a API já esteja rodando no terminal:
```bash
dotnet test --no-build CP5_FilmesApi.sln
```

**Resultado esperado:**
- `CP5.UnitTests`: 30 aprovados, 0 falhas
- `CP5.IntegrationTests`: 15 aprovados, 0 falhas
- **Total: 45 testes aprovados (100% de sucesso)**

---

## 7. Usuários Pré-cadastrados para Testes

| Usuário | Senha | Perfil (Role) | Permissão |
|---|---|---|---|
| `admin` | `Admin@123` | **Admin** | Acesso total aos endpoints (leitura e escrita) |
| `thomas` | `Fiap@2026` | **Admin** | Administrador (RM 562254) |
| `user` | `User@123` | **User** | Leitura e inclusão de avaliações |

---

## 8. Documentação dos Endpoints Principais

### Autenticação (`/api/auth`)

#### `POST /api/auth/login`
Autentica usuário e retorna token JWT assinado.
**Payload:**
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

---

### Filmes (`/api/filmes`)

#### `GET /api/filmes?pageNumber=1&pageSize=3`
Retorna catálogo paginado utilizando **PageResult** e enriquecido com links **HATEOAS**.
**Exemplo de Resposta (200 OK):**
```json
{
  "success": true,
  "message": "Consulta paginada concluída com sucesso. Total de registros: 5.",
  "data": {
    "items": [
      {
        "id": 1,
        "titulo": "Interestelar",
        "sinopse": "Uma equipe de exploradores viaja através de um buraco de minhoca no espaço.",
        "genero": "Ficção Científica",
        "anoLancamento": 2014,
        "notaImdb": 8.7,
        "duracaoMinutos": 169,
        "links": [
          { "rel": "self", "href": "http://localhost:5209/api/filmes/1", "method": "GET" },
          { "rel": "update", "href": "http://localhost:5209/api/filmes/1", "method": "PUT" },
          { "rel": "delete", "href": "http://localhost:5209/api/filmes/1", "method": "DELETE" },
          { "rel": "filme-avaliacoes", "href": "http://localhost:5209/api/avaliacoes/filme/1", "method": "GET" },
          { "rel": "external-info", "href": "http://localhost:5209/api/filmes/1/external-info", "method": "GET" }
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 3,
    "totalItems": 5,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

#### `GET /api/filmes/{id}/external-info`
Consulta informações detalhadas na API externa do TVMaze com resiliência (Polly + Fallback).

#### `POST /api/filmes`
Cadastra um novo filme. Requer perfil de Administrador (`Bearer <token>`).
**Payload:**
```json
{
  "titulo": "Oppenheimer",
  "sinopse": "A história do desenvolvimento da bomba atômica.",
  "genero": "Drama Histórico",
  "anoLancamento": 2023,
  "notaImdb": 8.9,
  "duracaoMinutos": 180
}
```

#### `POST /api/filmes/criar-com-retorno-erro`
Demonstra a abordagem de retorno de erro no repositório. Ao tentar cadastrar um filme já existente, retorna status `409 Conflict` via `OperationResult`.

#### `GET /api/filmes/buscar-com-erro-lancado/{id}`
Demonstra a abordagem de lançamento de erro no repositório. Ao buscar um ID inexistente, dispara `EntityNotFoundException` tratada pelo middleware com status `404 Not Found`.

#### `PUT /api/filmes/{id}`
Atualiza dados de um filme existente. Requer perfil de Administrador.

#### `DELETE /api/filmes/{id}`
Remove um filme e suas avaliações associadas em cascata. Requer perfil de Administrador.

---

### Avaliações (`/api/avaliacoes`)

#### `GET /api/avaliacoes/filme/{filmeId}`
Lista as avaliações de um filme com paginação e links HATEOAS.

#### `POST /api/avaliacoes`
Registra uma avaliação para um filme. Requer usuário autenticado.
**Payload:**
```json
{
  "filmeId": 1,
  "usuario": "Thomas Fontes",
  "comentario": "Trilha sonora e efeitos visuais espetaculares!",
  "nota": 5
}
```

#### `DELETE /api/avaliacoes/{id}`
Remove uma avaliação pelo ID. Requer perfil de Administrador.

---

### Health Checks e Monitoramento

- `GET /health/live`: Liveness probe vinculado estritamente à tag `live` (verificação `self`).
- `GET /health/ready`: Readiness probe vinculado à tag `ready` (banco de dados e serviço externo).
- `GET /health`: Diagnóstico completo de todos os serviços com status, tempo de execução e tags.