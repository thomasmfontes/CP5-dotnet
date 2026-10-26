using CP5.Application.DTOs.Auth;
using CP5.Application.Interfaces;
using CP5.Domain.Common;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CP5.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<OperationResult<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Tentativa de login para o usuário: {Username}", request.Username);

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return OperationResult<LoginResponseDto>.Failure(
                    "Usuário e senha são obrigatórios.", 400, "Credenciais incompletas.");
            }

            var user = await _userRepository.ValidarCredenciaisAsync(request.Username, request.Password);
            if (user == null)
            {
                _logger.LogWarning("Falha no login: credenciais inválidas para o usuário {Username}", request.Username);
                return OperationResult<LoginResponseDto>.Unauthorized("Nome de usuário ou senha inválidos.");
            }

            var (token, expiration) = _jwtTokenGenerator.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                Username = user.Username,
                Nome = user.Nome,
                Role = user.Role,
                Expiration = expiration
            };

            _logger.LogInformation("Login bem-sucedido para {Username} com perfil {Role}", user.Username, user.Role);
            return OperationResult<LoginResponseDto>.Success(response, "Autenticação realizada com sucesso.");
        }
    }
}
