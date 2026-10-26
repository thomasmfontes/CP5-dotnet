using CP5.Application.DTOs.Common;
using CP5.Application.DTOs.Filmes;
using CP5.Application.Interfaces;
using CP5.Application.Mappers;
using CP5.Domain.Common;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CP5.Application.Services
{
    public class FilmeService : IFilmeService
    {
        private readonly IFilmeRepository _filmeRepository;
        private readonly IExternalMovieService _externalMovieService;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<FilmeService> _logger;

        public FilmeService(
            IFilmeRepository filmeRepository,
            IExternalMovieService externalMovieService,
            IHateoasService hateoasService,
            ILogger<FilmeService> logger)
        {
            _filmeRepository = filmeRepository;
            _externalMovieService = externalMovieService;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        public async Task<OperationResult<PageResult<FilmeResponseDto>>> ObterPaginadoAsync(
            PagedRequestDto request, string? genero = null, string? baseUrl = null)
        {
            _logger.LogInformation("Consultando catálogo de filmes paginado: Página {PageNumber}, Tamanho {PageSize}, Gênero {Genero}",
                request.PageNumber, request.PageSize, genero ?? "Todos");

            var pagedEntities = await _filmeRepository.ObterTodosPaginadoAsync(request.PageNumber, request.PageSize, genero);

            var dtos = pagedEntities.Items.Select(f => f.ToResponseDto()).ToList();
            var pagedResult = new PageResult<FilmeResponseDto>(dtos, pagedEntities.TotalItems, pagedEntities.PageNumber, pagedEntities.PageSize);

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _hateoasService.EnriquecerListaFilmes(pagedResult, baseUrl, genero);
            }

            return OperationResult<PageResult<FilmeResponseDto>>.Success(
                pagedResult,
                $"Consulta paginada concluída com sucesso. Total de registros: {pagedEntities.TotalItems}.");
        }

        public async Task<OperationResult<FilmeResponseDto>> ObterPorIdAsync(int id, string? baseUrl = null)
        {
            _logger.LogInformation("Buscando filme por ID: {Id}", id);

            var filme = await _filmeRepository.ObterPorIdAsync(id);
            if (filme == null)
            {
                _logger.LogWarning("Filme com ID {Id} não encontrado no repositório.", id);
                return OperationResult<FilmeResponseDto>.NotFound($"Filme com ID {id} não foi encontrado.");
            }

            var dto = filme.ToResponseDto();
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _hateoasService.EnriquecerFilme(dto, baseUrl);
            }

            return OperationResult<FilmeResponseDto>.Success(dto);
        }

        public async Task<OperationResult<FilmeResponseDto>> CriarAsync(FilmeRequestDto dto, string? baseUrl = null)
        {
            _logger.LogInformation("Iniciando criação de filme: {Titulo} ({AnoLancamento})", dto.Titulo, dto.AnoLancamento);

            try
            {
                var filme = dto.ToEntity();

                // Validação de duplicidade antes da persistência
                var existeDuplicado = await _filmeRepository.ExistePorTituloEAnoAsync(filme.Titulo, filme.AnoLancamento);
                if (existeDuplicado)
                {
                    _logger.LogWarning("Tentativa de cadastrar filme duplicado: {Titulo} ({AnoLancamento})", filme.Titulo, filme.AnoLancamento);
                    return OperationResult<FilmeResponseDto>.Conflict($"Já existe um filme cadastrado com o título '{filme.Titulo}' no ano {filme.AnoLancamento}.");
                }

                var resultadoRepo = await _filmeRepository.AdicionarComResultadoAsync(filme);
                if (!resultadoRepo.IsSuccess || resultadoRepo.Data == null)
                {
                    return OperationResult<FilmeResponseDto>.Failure(resultadoRepo.Message, resultadoRepo.StatusCode, resultadoRepo.Errors.ToArray());
                }

                var responseDto = resultadoRepo.Data.ToResponseDto();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    _hateoasService.EnriquecerFilme(responseDto, baseUrl);
                }

                _logger.LogInformation("Filme criado com sucesso: ID {Id}", responseDto.Id);
                return OperationResult<FilmeResponseDto>.Created(responseDto, "Filme cadastrado com sucesso.");
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação de domínio ao criar filme: {Message}", ex.Message);
                return OperationResult<FilmeResponseDto>.Failure(ex.Message, 400, ex.Erros.ToArray());
            }
        }

        public async Task<OperationResult<FilmeResponseDto>> AtualizarAsync(int id, FilmeRequestDto dto, string? baseUrl = null)
        {
            _logger.LogInformation("Atualizando filme com ID: {Id}", id);

            try
            {
                var filmeExistente = await _filmeRepository.ObterPorIdAsync(id);
                if (filmeExistente == null)
                {
                    return OperationResult<FilmeResponseDto>.NotFound($"Filme com ID {id} não foi encontrado para atualização.");
                }

                var duplicado = await _filmeRepository.ExistePorTituloEAnoAsync(dto.Titulo, dto.AnoLancamento, id);
                if (duplicado)
                {
                    return OperationResult<FilmeResponseDto>.Conflict($"Já existe outro filme com o título '{dto.Titulo}' no ano {dto.AnoLancamento}.");
                }

                filmeExistente.AtualizarDados(
                    dto.Titulo, dto.Sinopse, dto.Genero, dto.AnoLancamento, dto.NotaImdb, dto.DuracaoMinutos, dto.PosterUrl);

                await _filmeRepository.AtualizarComLancamentoAsync(filmeExistente);

                var responseDto = filmeExistente.ToResponseDto();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    _hateoasService.EnriquecerFilme(responseDto, baseUrl);
                }

                return OperationResult<FilmeResponseDto>.Success(responseDto, "Filme atualizado com sucesso.");
            }
            catch (DomainValidationException ex)
            {
                return OperationResult<FilmeResponseDto>.Failure(ex.Message, 400, ex.Erros.ToArray());
            }
            catch (EntityNotFoundException ex)
            {
                return OperationResult<FilmeResponseDto>.NotFound(ex.Message);
            }
        }

        public async Task<OperationResult> RemoverAsync(int id)
        {
            _logger.LogInformation("Removendo filme com ID: {Id}", id);

            try
            {
                await _filmeRepository.RemoverComLancamentoAsync(id);
                return OperationResult.Success("Filme removido com sucesso.");
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Tentativa de exclusão de filme inexistente: {Message}", ex.Message);
                return OperationResult.NotFound(ex.Message);
            }
        }

        public async Task<OperationResult<ExternalMovieDetails>> ObterInfoExternaAsync(int id)
        {
            _logger.LogInformation("Buscando informações em API externa para o filme ID {Id}", id);

            var filme = await _filmeRepository.ObterPorIdAsync(id);
            if (filme == null)
            {
                return OperationResult<ExternalMovieDetails>.NotFound($"Filme com ID {id} não encontrado.");
            }

            // Consumo de serviço externo via Infrastructure com resiliência
            var resultadoExterno = await _externalMovieService.ObterDetalhesExternosAsync(filme.Titulo);
            return resultadoExterno;
        }

        // Requisito: Lançamento de erro customizado no Repository
        public async Task<OperationResult<FilmeResponseDto>> BuscarComErroLancadoAsync(int id)
        {
            _logger.LogInformation("Executando método de demonstração de Lançamento de Erro no Repository para o ID {Id}", id);
            
            // O repositório lançará explicitamente EntityNotFoundException se não encontrar
            var filme = await _filmeRepository.BuscarComErroLancadoAsync(id);
            return OperationResult<FilmeResponseDto>.Success(filme.ToResponseDto());
        }

        // Requisito: Retorno de erro no Repository (sem lançar exception)
        public async Task<OperationResult<FilmeResponseDto>> CriarComRetornoErroRepositoryAsync(FilmeRequestDto dto)
        {
            _logger.LogInformation("Executando método de demonstração de Retorno de Erro no Repository");

            var filme = dto.ToEntity();
            var resultadoRepo = await _filmeRepository.AdicionarComResultadoAsync(filme);

            if (!resultadoRepo.IsSuccess)
            {
                return OperationResult<FilmeResponseDto>.Failure(resultadoRepo.Message, resultadoRepo.StatusCode, resultadoRepo.Errors.ToArray());
            }

            return OperationResult<FilmeResponseDto>.Created(resultadoRepo.Data!.ToResponseDto());
        }
    }
}
