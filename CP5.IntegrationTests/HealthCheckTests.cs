using System.Net;
using System.Text.Json;
using Xunit;

namespace CP5.IntegrationTests
{
    public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HealthCheckTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHealthLive_DeveRetornar200EConterSelfCheckComTagLive()
        {
            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            Assert.Equal("Healthy", root.GetProperty("status").GetString());

            var entries = root.GetProperty("entries").EnumerateArray().ToList();
            Assert.NotEmpty(entries);

            // Validação estrita do feedback recebido: self check vinculado à tag 'live'
            var selfCheck = entries.FirstOrDefault(e => e.GetProperty("name").GetString() == "self");
            Assert.True(selfCheck.ValueKind != JsonValueKind.Undefined, "A verificação 'self' com a tag 'live' deve estar presente na rota /health/live.");

            var tags = selfCheck.GetProperty("tags").EnumerateArray().Select(t => t.GetString()).ToList();
            Assert.Contains("live", tags);
        }

        [Fact]
        public async Task GetHealthReady_DeveRetornar200EConterDependenciasComTagReady()
        {
            // Act
            var response = await _client.GetAsync("/health/ready");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            Assert.Equal("Healthy", root.GetProperty("status").GetString());

            var entries = root.GetProperty("entries").EnumerateArray().ToList();
            Assert.NotEmpty(entries);

            var dbCheck = entries.FirstOrDefault(e => e.GetProperty("name").GetString() == "database");
            Assert.True(dbCheck.ValueKind != JsonValueKind.Undefined, "A dependência de banco com a tag 'ready' deve estar presente na rota /health/ready.");
        }

        [Fact]
        public async Task GetHealth_DeveRetornarDiagnosticoCompleto()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            Assert.Equal("Healthy", root.GetProperty("status").GetString());
            Assert.True(root.TryGetProperty("totalDuration", out _));
            Assert.True(root.TryGetProperty("entries", out _));
        }
    }
}
