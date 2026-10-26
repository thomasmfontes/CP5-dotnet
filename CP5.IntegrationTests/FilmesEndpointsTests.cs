using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CP5.IntegrationTests
{
    public class FilmesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public FilmesEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> ObterTokenJwtAsync(string username, string password)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new { username, password });
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("data").GetProperty("token").GetString()!;
        }

        [Fact]
        public async Task GetFilmes_SemAutenticacao_DeveRetornar200ComPageResultEHateoas()
        {
            // Act
            var response = await _client.GetAsync("/api/filmes?pageNumber=1&pageSize=5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            Assert.True(root.GetProperty("success").GetBoolean());
            var data = root.GetProperty("data");

            // Validação de PageResult estruturado
            Assert.True(data.GetProperty("pageNumber").GetInt32() == 1);
            Assert.True(data.GetProperty("pageSize").GetInt32() == 5);
            Assert.True(data.GetProperty("totalItems").GetInt32() > 0);
            Assert.True(data.GetProperty("totalPages").GetInt32() >= 1);

            // Validação de HATEOAS nos itens
            var items = data.GetProperty("items").EnumerateArray().ToList();
            Assert.NotEmpty(items);

            var primeiroFilme = items.First();
            var links = primeiroFilme.GetProperty("links").EnumerateArray().ToList();
            Assert.NotEmpty(links);
            Assert.Contains(links, l => l.GetProperty("rel").GetString() == "self");
            Assert.Contains(links, l => l.GetProperty("rel").GetString() == "external-info");
        }

        [Fact]
        public async Task GetFilmePorId_QuandoExistente_DeveRetornar200ComLinks()
        {
            // Act
            var response = await _client.GetAsync("/api/filmes/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var data = doc.RootElement.GetProperty("data");

            Assert.Equal(1, data.GetProperty("id").GetInt32());
            Assert.Equal("Interestelar", data.GetProperty("titulo").GetString());

            var links = data.GetProperty("links").EnumerateArray().ToList();
            Assert.NotEmpty(links);
            Assert.Contains(links, l => l.GetProperty("rel").GetString() == "self");
            Assert.Contains(links, l => l.GetProperty("rel").GetString() == "update");
            Assert.Contains(links, l => l.GetProperty("rel").GetString() == "delete");
        }

        [Fact]
        public async Task GetFilmePorId_QuandoInexistente_DeveRetornar404()
        {
            // Act
            var response = await _client.GetAsync("/api/filmes/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostFilme_SemToken_DeveRetornar401Unauthorized()
        {
            var payload = new
            {
                titulo = "Matrix",
                sinopse = "Um hacker descobre a verdade sobre sua realidade.",
                genero = "Ficção Científica",
                anoLancamento = 1999,
                notaImdb = 8.7,
                duracaoMinutos = 136
            };

            var response = await _client.PostAsJsonAsync("/api/filmes", payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostFilme_ComTokenDeUsuarioComum_DeveRetornar403Forbidden()
        {
            var tokenUser = await ObterTokenJwtAsync("user", "User@123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUser);

            var payload = new
            {
                titulo = "Matrix Reloaded",
                sinopse = "Sequência de Matrix.",
                genero = "Ficção Científica",
                anoLancamento = 2003,
                notaImdb = 7.2,
                duracaoMinutos = 138
            };

            var response = await _client.PostAsJsonAsync("/api/filmes", payload);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PostFilme_ComTokenAdmin_DeveRetornar201Created()
        {
            var tokenAdmin = await ObterTokenJwtAsync("admin", "Admin@123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

            var payload = new
            {
                titulo = "Gladiador",
                sinopse = "Um general romano é traído e busca vingança.",
                genero = "Ação",
                anoLancamento = 2000,
                notaImdb = 8.5,
                duracaoMinutos = 155
            };

            var response = await _client.PostAsJsonAsync("/api/filmes", payload);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var data = doc.RootElement.GetProperty("data");

            Assert.Equal("Gladiador", data.GetProperty("titulo").GetString());
            Assert.True(data.GetProperty("id").GetInt32() > 0);
        }

        [Fact]
        public async Task GetExternalInfo_DeveRetornar200ComDadosExternosResilientes()
        {
            // Act
            var response = await _client.GetAsync("/api/filmes/1/external-info");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var data = doc.RootElement.GetProperty("data");

            Assert.NotEmpty(data.GetProperty("title").GetString()!);
            Assert.NotEmpty(data.GetProperty("sourceApi").GetString()!);
        }

        [Fact]
        public async Task BuscarComErroLancado_QuandoInexistente_DeveLancarExcecaoNoRepoERetornar404()
        {
            // Act - Testa o requisito específico de lançamento de erro customizado no repositório
            var response = await _client.GetAsync("/api/filmes/buscar-com-erro-lancado/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            Assert.Equal("Entidade Não Encontrada", root.GetProperty("title").GetString());
        }

        [Fact]
        public async Task CriarComRetornoErro_QuandoDuplicado_DeveRetornarConflictViaOperationResult()
        {
            var tokenAdmin = await ObterTokenJwtAsync("admin", "Admin@123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

            // Tenta criar um filme já existente (Interestelar, 2014)
            var payload = new
            {
                titulo = "Interestelar",
                sinopse = "Exploradores viajam pelo espaço.",
                genero = "Ficção Científica",
                anoLancamento = 2014,
                notaImdb = 8.7,
                duracaoMinutos = 169
            };

            var response = await _client.PostAsJsonAsync("/api/filmes/criar-com-retorno-erro", payload);

            // Assert - Deve retornar 409 Conflict diretamente do repositório através do OperationResult
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            Assert.False(root.GetProperty("success").GetBoolean());
            Assert.Contains("Conflito de unicidade", root.GetProperty("message").GetString());
        }
    }
}
