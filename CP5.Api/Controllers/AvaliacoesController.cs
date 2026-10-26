using CP5.Application.DTOs.Avaliacoes;
using CP5.Application.DTOs.Common;
using CP5.Application.Interfaces;
using CP5.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Api.Controllers
{
    [Route("api/[controller]")]
    public class AvaliacoesController : ApiControllerBase
    {
        private readonly IAvaliacaoService _avaliacaoService;

        public AvaliacoesController(IAvaliacaoService avaliacaoService)
        {
            _avaliacaoService = avaliacaoService;
        }

        /// <summary>
        /// Obtém as avaliações paginadas de um filme com metadados estruturados e links HATEOAS.
        /// </summary>
        [HttpGet("filme/{filmeId:int}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lista avaliações de um filme com paginação e HATEOAS")]
        [ProducesResponseType(typeof(OperationResult<PageResult<AvaliacaoResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorFilme(int filmeId, [FromQuery] PagedRequestDto request)
        {
            var baseUrl = GetBaseUrl();
            var result = await _avaliacaoService.ObterPorFilmePaginadoAsync(filmeId, request, baseUrl);
            return HandleResult(result);
        }

        /// <summary>
        /// Obtém os detalhes de uma avaliação individual pelo ID.
        /// </summary>
        [HttpGet("{id:int}", Name = "ObterAvaliacaoPorId")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obtém avaliação por ID")]
        [ProducesResponseType(typeof(OperationResult<AvaliacaoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var baseUrl = GetBaseUrl();
            var result = await _avaliacaoService.ObterPorIdAsync(id, baseUrl);
            return HandleResult(result);
        }

        /// <summary>
        /// Registra uma nova avaliação para determinado filme. Requer autenticação (qualquer usuário logado).
        /// </summary>
        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Registra avaliação [Requer Autenticação]")]
        [ProducesResponseType(typeof(OperationResult<AvaliacaoResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Criar([FromBody] AvaliacaoRequestDto dto)
        {
            var baseUrl = GetBaseUrl();
            var result = await _avaliacaoService.CriarAsync(dto, baseUrl);
            return HandleResult(result, "ObterAvaliacaoPorId", new { id = result.Data?.Id });
        }

        /// <summary>
        /// Remove uma avaliação pelo ID. Requer perfil de Administrador.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Exclui avaliação [Requer Admin]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(int id)
        {
            var result = await _avaliacaoService.RemoverAsync(id);
            return HandleResult(result);
        }
    }
}
