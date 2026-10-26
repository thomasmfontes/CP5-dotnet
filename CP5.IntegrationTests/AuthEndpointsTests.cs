using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CP5.IntegrationTests
{
    public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointsTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ComCredenciaisValidasAdmin_DeveRetornar200ETokenJWT()
        {
            var payload = new { username = "admin", password = "Admin@123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            Assert.True(root.GetProperty("success").GetBoolean());
            var data = root.GetProperty("data");
            Assert.NotEmpty(data.GetProperty("token").GetString()!);
            Assert.Equal("Admin", data.GetProperty("role").GetString());
        }

        [Fact]
        public async Task Login_ComCredenciaisValidasUser_DeveRetornar200EPerfilUser()
        {
            var payload = new { username = "user", password = "User@123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var data = doc.RootElement.GetProperty("data");

            Assert.NotEmpty(data.GetProperty("token").GetString()!);
            Assert.Equal("User", data.GetProperty("role").GetString());
        }

        [Fact]
        public async Task Login_ComSenhaInvalida_DeveRetornar401Unauthorized()
        {
            var payload = new { username = "admin", password = "SenhaErrada_123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
