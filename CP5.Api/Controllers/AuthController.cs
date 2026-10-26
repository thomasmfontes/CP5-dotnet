using CP5.Application.DTOs.Auth;
using CP5.Application.Interfaces;
using CP5.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Autentica o usuário com credenciais e retorna um Token JWT com as permissões associadas.
        /// </summary>
        /// <remarks>
        /// Credenciais pré-cadastradas para demonstração e testes:
        /// - Administrador: `admin` / `Admin@123` (Role: Admin)
        /// - Administrador Aluno: `thomas` / `Fiap@2026` (Role: Admin - RM562254)
        /// - Usuário Comum: `user` / `User@123` (Role: User)
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Autenticação e geração de JWT Bearer Token", Description = "Gera token assinado com claims de usuário, role e RM para acesso aos endpoints protegidos.")]
        [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return HandleResult(result);
        }
    }
}
