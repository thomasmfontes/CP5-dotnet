using CP5.Application.DTOs.Common;
using CP5.Application.DTOs.Filmes;
using CP5.Application.Interfaces;
using CP5.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Api.Controllers
{
    [Route("api/[controller]")]
    public class FilmesController : ApiControllerBase
    {
        private readonly IFilmeService _filmeService;

        public FilmesController(IFilmeService filmeService)
        {
            _filmeService = filmeService;
        }

        /// <summary>
        /// Obtém o catálogo paginado de filmes com metadados estruturados (PageResult) e links hipermídia (HATEOAS).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lista filmes paginados com HATEOAS", Description = "Retorna uma lista paginada utilizando PageResult, com AsNoTracking() e enriquecimento HATEOAS.")]
        [ProducesResponseType(typeof(OperationResult<PageResult<FilmeResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterPaginado([FromQuery] PagedRequestDto request, [FromQuery] string? genero = null)
        {
            var baseUrl = GetBaseUrl();
            var result = await _filmeService.ObterPaginadoAsync(request, genero, baseUrl);
            return HandleResult(result);
        }

        /// <summary>
        /// Obtém um filme específico pelo ID com links de navegação HATEOAS.
        /// </summary>
        [HttpGet("{id:int}", Name = "ObterFilmePorId")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Obtém filme por ID", Description = "Retorna os detalhes de um filme e seus links de navegação HATEOAS.")]
        [ProducesResponseType(typeof(OperationResult<FilmeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var baseUrl = GetBaseUrl();
            var result = await _filmeService.ObterPorIdAsync(id, baseUrl);
            return HandleResult(result);
        }

        /// <summary>
        /// Consulta informações externas do filme consumindo uma API REST pública (TVMaze) com resiliência (Polly) e fallback.
        /// </summary>
        [HttpGet("{id:int}/external-info")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Consulta serviço externo resiliente", Description = "Integração via HttpClientFactory com política de resiliência e fallback gracioso.")]
        [ProducesResponseType(typeof(OperationResult<ExternalMovieDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterInfoExterna(int id)
        {
            var result = await _filmeService.ObterInfoExternaAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Cadastra um novo filme. Requer perfil de Administrador (Bearer JWT Token).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Cadastra novo filme [Requer Admin]", Description = "Criação de filme com validações de domínio e retorno de erro ou sucesso via OperationResult.")]
        [ProducesResponseType(typeof(OperationResult<FilmeResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Criar([FromBody] FilmeRequestDto dto)
        {
            var baseUrl = GetBaseUrl();
            var result = await _filmeService.CriarAsync(dto, baseUrl);
            return HandleResult(result, "ObterFilmePorId", new { id = result.Data?.Id });
        }

        /// <summary>
        /// Atualiza os dados de um filme cadastrado. Requer perfil de Administrador.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Atualiza filme existente [Requer Admin]")]
        [ProducesResponseType(typeof(OperationResult<FilmeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] FilmeRequestDto dto)
        {
            var baseUrl = GetBaseUrl();
            var result = await _filmeService.AtualizarAsync(id, dto, baseUrl);
            return HandleResult(result);
        }

        /// <summary>
        /// Remove um filme e suas avaliações associadas. Requer perfil de Administrador.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Exclui filme [Requer Admin]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(int id)
        {
            var result = await _filmeService.RemoverAsync(id);
            return HandleResult(result);
        }

        // ========================================================================================
        // ENDPOINTS ESPECÍFICOS DE DEMONSTRAÇÃO DOS REQUISITOS DO EDITAL CP5:
        // ========================================================================================

        /// <summary>
        /// Demonstração do Requisito: Lançamento de Erro Customizado no Repository (Exceptions).
        /// </summary>
        [HttpGet("buscar-com-erro-lancado/{id:int}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Demonstração: Lançamento de Erro no Repository", Description = "Chama o repositório que lança EntityNotFoundException tratada pelo GlobalExceptionMiddleware.")]
        public async Task<IActionResult> BuscarComErroLancado(int id)
        {
            var result = await _filmeService.BuscarComErroLancadoAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Demonstração do Requisito: Retorno de Erro no Repository (OperationResult).
        /// </summary>
        [HttpPost("criar-com-retorno-erro")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Demonstração: Retorno de Erro no Repository", Description = "Chama o repositório que retorna OperationResult com erro de validação/duplicidade sem lançar exceção.")]
        public async Task<IActionResult> CriarComRetornoErro([FromBody] FilmeRequestDto dto)
        {
            var result = await _filmeService.CriarComRetornoErroRepositoryAsync(dto);
            return HandleResult(result);
        }
    }
}
