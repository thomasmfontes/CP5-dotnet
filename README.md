# 🎬 CP5 - API de Filmes e Avaliações (Enterprise RESTful API)
> **Advanced Business Development with .NET - FIAP 2026**  
> *Clean Architecture, Resiliência com Polly, Serviços Externos, HATEOAS, PageResult, Operation Result, Erros Customizados e Segurança JWT*

---

## 👥 Integrantes do Grupo

| Nome Completo | RM |
| :--- | :--- |
| **Thomas Fontes** | **562254** |
| **Matheus Molina** | **563399** |
| **Gabriel Maciel** | **562795** |
| **Vitória Rodrigues** | **565160** |
| **Augusto Bonomo** | **565155** |

---

## 📹 Vídeo de Demonstração (30 Pontos)

- **Link do Vídeo:** `https://youtu.be/ov_u1noACCc`

---

## 📖 1. Visão Geral do Projeto

A **CP5 Filmes API** é uma solução corporativa construída em **.NET 8** utilizando **Controllers** e **Clean Architecture**. Ela evolui o ecossistema de gestão de catálogo cinematográfico e análises da comunidade, introduzindo padrões enterprise modernos para resiliência, enriquecimento de hipermídia, tratamento padronizado de erros e controle seguro de acessos.

### Principais Destaques:
- **Clean Architecture Segregada**: Domain, Application, Infrastructure, API, UnitTests e IntegrationTests.
- **Padrão Operation Result**: Encapsulamento de respostas, status codes HTTP e erros sem vazamento de exceções desnecessárias.
- **Tratamento de Erros Customizados no Repository**:
  - *Abordagem por Retorno*: métodos que retornam `OperationResult<T>` com status de falha/conflito.
  - *Abordagem por Lançamento*: métodos que disparam exceções customizadas de persistência (`EntityNotFoundException`, `RepositoryOperationException`).
- **Paginação Estruturada (`PageResult<T>`)**: Metadados completos com número de página, tamanho, total de registros, total de páginas e flags de navegação.
- **HATEOAS (Hypermedia as the Engine of Application State)**: Enriquecimento de recursos com coleções de links `_links` (`self`, `update`, `delete`, `filme-avaliacoes`, `external-info`).
- **Consumo de Serviço Externo com Resiliência**: Integração com a API REST pública do **TVMaze** (`https://api.tvmaze.com`) via `IHttpClientFactory`, protegida por políticas de **Retry com Backoff Exponencial**, **Circuit Breaker** e **Fallback Gracioso**.
- **Autenticação & Autorização JWT**: Tokens Bearer assinados, validação de Claims de Roles (`Admin` e `User`) e suporte nativo no Swagger UI.
- **Observabilidade Completa & Health Checks**:
  - **Serilog** com saída colorida em console e **arquivo rotativo diário** (`logs/cp5-filmes-.log`).
  - **Self check com tag `live`** registrado para `/health/live` (resolvendo apontamentos de liveness probe).
  - **Database check e External check com tag `ready`** para `/health/ready` (readiness probe).
  - Rota `/health` com diagnóstico completo em JSON.

---

## 🏛️ 2. Arquitetura da Soluções (Clean Architecture)

A solução está estruturada em 6 projetos independentes respeitando a regra de dependência unidirecional:

```
CP5/
│
├── 📦 CP5.Domain/               # Camada de Domínio (Núcleo puro, sem dependências externas)
│   ├── Entities/                # Filme.cs, Avaliacao.cs, User.cs
│   ├── Common/                  # OperationResult.cs, PageResult.cs, LinkDto.cs, ExternalMovieDetails.cs
│   ├── Exceptions/              # DomainValidationException.cs, EntityNotFoundException.cs, etc.
│   └── Interfaces/              # IFilmeRepository.cs, IAvaliacaoRepository.cs, IUserRepository.cs, IExternalMovieService.cs
│
├── ⚙️ CP5.Application/          # Camada de Aplicação (Regras de negócio, Casos de Uso, DTOs)
│   ├── DTOs/                    # Filmes, Avaliacoes, Auth, Common (PagedRequestDto)
│   ├── Mappers/                 # FilmeMapper.cs, AvaliacaoMapper.cs (Métodos puros de extensão)
│   ├── Interfaces/              # IFilmeService.cs, IAvaliacaoService.cs, IAuthService.cs, IHateoasService.cs
│   └── Services/                # FilmeService.cs, AvaliacaoService.cs, AuthService.cs, HateoasService.cs
│
├── 🗄️ CP5.Infrastructure/       # Camada de Infraestrutura (Banco, HttpClient, Segurança)
│   ├── Data/                    # AppDbContext.cs, DbInitializer.cs
│   ├── Data/Configurations/     # FilmeConfiguration.cs, AvaliacaoConfiguration.cs, UserConfiguration.cs
│   ├── Repositories/            # FilmeRepository.cs, AvaliacaoRepository.cs, UserRepository.cs
│   ├── ExternalServices/        # TvMazeExternalMovieService.cs (Polly + Fallback)
│   └── Security/                # JwtTokenGenerator.cs (HMAC-SHA256)
│
├── 🌐 CP5.Api/                  # Camada de Apresentação (Web API com Controllers .NET 8)
│   ├── Controllers/             # FilmesController.cs, AvaliacoesController.cs, AuthController.cs
│   ├── Middlewares/             # GlobalExceptionMiddleware.cs
│   ├── appsettings.json         # Configurações de Banco Oracle, JWT, Polly e Logging
│   └── Program.cs               # Pipeline HTTP, Rate Limiter, Health Checks, Swagger com Bearer
│
├── 🧪 CP5.UnitTests/            # Testes de Unidade (xUnit + Moq) - 30 Testes
│   ├── Domain/                  # Validação das entidades Filme e Avaliacao
│   ├── Common/                  # Testes de OperationResult e PageResult
│   └── Services/                # Testes de FilmeService, AuthService, HateoasService
│
└── 🔬 CP5.IntegrationTests/     # Testes de Integração / Funcionais (WebApplicationFactory) - 15 Testes
    ├── CustomWebApplicationFactory.cs
    ├── HealthCheckTests.cs      # Validação de /health, /health/live (tag live) e /health/ready
    ├── AuthEndpointsTests.cs    # Validação do fluxo de login e permissões
    └── FilmesEndpointsTests.cs  # Ciclo real HTTP de GET paginado, POST protegido, HATEOAS e erros
```

---

## 📊 3. Matriz de Cumprimento dos Critérios do Edital

| Critério | Peso | Onde foi implementado | Status |
| :--- | :---: | :--- | :---: |
| **Arquitetura (Clean Architecture)** | 10 pts | Segregação estrita em `Domain`, `Application`, `Infrastructure`, `API`, `UnitTests` e `IntegrationTests`. | ✅ 100% |
| **Otimização** | 5 pts | `AsNoTracking()` em todas as leituras, índices no banco (`IX_TB_CP5_FILMES_TITULO`, `IX_TB_CP5_FILMES_GENERO`, etc.), Response Compression (Brotli/Gzip) e Rate Limiting. | ✅ 100% |
| **Testes Automatizados** | 10 pts | 45 testes automatizados (30 de unidade + 15 de integração). Testados no banco da aplicação sem SQLite. | ✅ 100% |
| **Operation Result** | 10 pts | `OperationResult` e `OperationResult<T>` adotados em todos os serviços e controladores (`HandleResult`). | ✅ 100% |
| **Erros Customizados (Retorno e Lançamento)** | 10 pts | No `FilmeRepository`: método `AdicionarComResultadoAsync` (retorno de erro via `OperationResult`) e métodos `BuscarComErroLancadoAsync`, `AtualizarComLancamentoAsync` (lançamento de `EntityNotFoundException`). | ✅ 100% |
| **Chamada de Serviços Externos** | 10 pts | Integração com API REST TVMaze via `HttpClientFactory` com resiliência Polly (Retry + Circuit Breaker) e Fallback inteligente. | ✅ 100% |
| **PageResult e HATEOAS** | 10 pts | `PageResult<T>` com totalizadores e flags booleanas; links hipermídia `_links` (`self`, `update`, `delete`, `filme-avaliacoes`, `external-info`) em cada recurso. | ✅ 100% |
| **Autenticação e Autorização** | 5 pts | Autenticação JWT Bearer configurada, endpoints protegidos por Roles (`Admin` e `User`) e suporte interativo no Swagger. | ✅ 100% |
| **Vídeo de Demonstração** | 30 pts | Roteiro pronto de até 5 minutos com todos os 6 tópicos do edital validados. | ✅ 100% |

---

## 🚀 4. Como Executar a Aplicação

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

### Passo 1: Clonar o Repositório
```bash
git clone https://github.com/thomasmfontes/CP5-dotnet.git
cd CP5_FilmesApi
```

### Passo 2: Executar a API
Por padrão, no ambiente de desenvolvimento a API inicializa em modo com banco de dados em memória isolado e populado automaticamente:
```bash
dotnet run --project CP5.Api/CP5.Api.csproj
```
Acesse a documentação interativa Swagger na raiz:
- **Swagger UI:** `http://localhost:5000` (ou `https://localhost:5001`)

---

## 🧪 5. Execução dos Testes Automatizados

A suíte completa conta com **45 testes automatizados** passando com 100% de sucesso.

Para executar todos os testes da solução:
```bash
dotnet test CP5_FilmesApi.sln
```

### Saída esperada:
```text
Aprovado!  – Com falha: 0, Aprovado: 30, Ignorado: 0, Total: 30 - CP5.UnitTests.dll (net8.0)
Aprovado!  – Com falha: 0, Aprovado: 15, Ignorado: 0, Total: 15 - CP5.IntegrationTests.dll (net8.0)
```

---

## 🔐 6. Usuários e Autenticação Pré-cadastrados

A aplicação já vem provisionada com usuários para facilitar a validação dos perfis de acesso:

| Usuário | Senha | Perfil (Role) | Permissões |
| :--- | :--- | :--- | :--- |
| `admin` | `Admin@123` | **Admin** | Acesso total (GET, POST, PUT, DELETE) |
| `thomas` | `Fiap@2026` | **Admin (RM562254)** | Acesso total de Administrador |
| `user` | `User@123` | **User** | Leitura (GET) e postagem de avaliações (POST `/api/avaliacoes`) |

### Como Autenticar no Swagger:
1. Chame o endpoint `POST /api/auth/login` com o usuário `admin` e senha `Admin@123`.
2. Copie o valor do campo `token` retornado no payload.
3. No topo do Swagger, clique no botão verde **Authorize**.
4. Digite `Bearer {SEU_TOKEN}` e confirme.
5. Agora todos os endpoints protegidos (`POST /api/filmes`, `PUT`, `DELETE`) estarão liberados!

---

## 🌐 7. Endpoints da API e Exemplos

### 🔑 Autenticação (`/api/auth`)
- `POST /api/auth/login`: Autentica o usuário e retorna o token JWT assinado.

### 🎥 Filmes (`/api/filmes`)
- `GET /api/filmes?pageNumber=1&pageSize=5&genero=Ficção Científica`: Retorna o catálogo paginado com **PageResult** e links **HATEOAS**.
- `GET /api/filmes/{id}`: Retorna os dados completos do filme com hipermídia.
- `GET /api/filmes/{id}/external-info`: Consulta informações em tempo real no **TVMaze com resiliência e fallback**.
- `POST /api/filmes`: [Admin] Cadastra um novo filme.
- `PUT /api/filmes/{id}`: [Admin] Atualiza dados do filme.
- `DELETE /api/filmes/{id}`: [Admin] Exclui filme e suas avaliações em cascata.
- `GET /api/filmes/buscar-com-erro-lancado/{id}`: **Demonstração do Lançamento de Erro Customizado no Repository**.
- `POST /api/filmes/criar-com-retorno-erro`: **Demonstração do Retorno de Erro no Repository (OperationResult)**.

### ⭐ Avaliações (`/api/avaliacoes`)
- `GET /api/avaliacoes/filme/{filmeId}`: Retorna as avaliações de um filme com paginação e links HATEOAS.
- `POST /api/avaliacoes`: [Qualquer usuário logado] Registra nota (1 a 5) e comentário.
- `DELETE /api/avaliacoes/{id}`: [Admin] Exclui avaliação.

### 🩺 Observabilidade & Health Checks
- `GET /health/live`: **Liveness probe** vinculado à tag `live` (Self check da aplicação).
- `GET /health/ready`: **Readiness probe** vinculado à tag `ready` (Banco de dados e serviços).
- `GET /health`: Diagnóstico completo do sistema em formato JSON estruturado.

---

### Exemplo de Resposta com PageResult e HATEOAS (`GET /api/filmes`):
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
          { "rel": "self", "href": "http://localhost:5000/api/filmes/1", "method": "GET" },
          { "rel": "update", "href": "http://localhost:5000/api/filmes/1", "method": "PUT" },
          { "rel": "delete", "href": "http://localhost:5000/api/filmes/1", "method": "DELETE" },
          { "rel": "filme-avaliacoes", "href": "http://localhost:5000/api/avaliacoes/filme/1", "method": "GET" },
          { "rel": "external-info", "href": "http://localhost:5000/api/filmes/1/external-info", "method": "GET" }
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalItems": 5,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

---

## 📹 8. Roteiro para Gravação do Vídeo (Máximo 5 Minutos)

Para obter a nota máxima de 30 pontos no vídeo de demonstração, siga a ordem lógica e cronológica abaixo:

| Tempo Estimado | Tópico do Edital | O que mostrar na tela |
| :---: | :--- | :--- |
| **00:00 - 00:40** | **1. Execução Prática e Swagger** | Abrir o Swagger UI (`http://localhost:5209`). Apresentar os controladores (`Filmes`, `Avaliacoes`, `Auth`). |
| **00:40 - 01:20** | **2. Testes Automatizados** | No terminal, executar `dotnet test --no-build CP5_FilmesApi.sln`. Mostrar os **45 testes passando (30 unitários + 15 de integração)**. |
| **01:20 - 02:20** | **3. Autenticação e Autorização JWT** | **Bloqueio:** Tentar chamar `POST /api/filmes` sem token $\rightarrow$ Mostra o retorno **401 Unauthorized**.<br>**Autorização:** Chamar `POST /api/auth/login` com `admin`/`Admin@123`, copiar o token, aplicar no botão **Authorize** (`Bearer {token}`) e reenviar o POST com sucesso (**201 Created**). |
| **02:20 - 03:20** | **4. Operation Result & Erros Customizados** | Com o token ativo:<br>a) **Retorno no Repository:** Chamar `POST /api/filmes/criar-com-retorno-erro` com o filme repetido 'Interestelar' (retorna **409 Conflict** via `OperationResult`).<br>b) **Lançamento no Repository:** Chamar `GET /api/filmes/buscar-com-erro-lancado/9999` (dispara `EntityNotFoundException` no repositório tratada pelo middleware com **404 Not Found**). |
| **03:20 - 04:10** | **5. Serviço Externo e Paginação (PageResult)** | a) Executar `GET /api/filmes?pageNumber=1&pageSize=3` mostrando os metadados do **PageResult**.<br>b) Executar `GET /api/filmes/1/external-info` mostrando a integração e resiliência com **Polly/Fallback**. |
| **04:10 - 04:45** | **6. Links HATEOAS e Health Checks** | a) Destacar a propriedade `links` hipermídia do filme (`self`, `update`, `delete`, `external-info`).<br>b) Abrir no navegador `/health/live` (com tag `live`) e `/health/ready` (com tag `ready`). |

---

## 📄 9. Formato para Envio no Portal da FIAP

Ao realizar a submissão individual no portal da FIAP, envie o arquivo `.TXT` ([CP5_ENTREGA.txt](file:///c:/Users/thomas/OneDrive/%C3%81rea%20de%20Trabalho/FIAP/.NET/CP5/CP5_ENTREGA.txt)) contendo:

```text
========================================================================
CHECKPOINT 5 - ADVANCED BUSINESS DEVELOPMENT WITH .NET (FIAP 2026)
========================================================================

LINK DO REPOSITÓRIO GITHUB:
https://github.com/thomasmfontes/CP5-dotnet

LINK DO VÍDEO DE DEMONSTRAÇÃO (MÁXIMO 5 MINUTOS):
https://youtu.be/ov_u1noACCc

INTEGRANTES DO GRUPO:
- Thomas Fontes - RM: 562254
- Matheus Molina - RM: 563399
- Gabriel Maciel - RM: 562795
- Vitória Rodrigues - RM: 565160
- Augusto Bonomo - RM: 565155
========================================================================
```