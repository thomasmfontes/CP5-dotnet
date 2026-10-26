using CP5.Application.DTOs.Auth;
using CP5.Application.Interfaces;
using CP5.Application.Services;
using CP5.Domain.Entities;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CP5.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtTokenGenerator> _jwtMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtMock = new Mock<IJwtTokenGenerator>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _service = new AuthService(_userRepoMock.Object, _jwtMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LoginAsync_QuandoCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            _userRepoMock.Setup(r => r.ValidarCredenciaisAsync("usuario", "senha_errada"))
                .ReturnsAsync((User?)null);

            var result = await _service.LoginAsync(new LoginRequestDto { Username = "usuario", Password = "senha_errada" });

            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_QuandoCredenciaisValidas_DeveRetornarTokenJWT()
        {
            var user = new User("admin", "hash", "Administrador", "Admin") { Id = 1 };
            _userRepoMock.Setup(r => r.ValidarCredenciaisAsync("admin", "Admin@123"))
                .ReturnsAsync(user);

            var exp = DateTime.UtcNow.AddHours(4);
            _jwtMock.Setup(j => j.GenerateToken(user)).Returns(("jwt_token_valido", exp));

            var result = await _service.LoginAsync(new LoginRequestDto { Username = "admin", Password = "Admin@123" });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("jwt_token_valido", result.Data!.Token);
            Assert.Equal("Admin", result.Data.Role);
        }
    }
}
