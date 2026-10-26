using CP5.Application.DTOs.Avaliacoes;
using CP5.Application.DTOs.Common;
using CP5.Application.Interfaces;
using CP5.Application.Mappers;
using CP5.Domain.Common;
using CP5.Domain.Exceptions;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CP5.Application.Services
{
    public class AvaliacaoService : IAvaliacaoService
    {
        private readonly IAvaliacaoRepository _avaliacaoRepository;
        private readonly IFilmeRepository _filmeRepository;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<AvaliacaoService> _logger;

        public AvaliacaoService(
            IAvaliacaoRepository avaliacaoRepository,
            IFilmeRepository filmeRepository,
            IHateoasService hateoasService,
            ILogger<AvaliacaoService> logger)
        {
            _avaliacaoRepository = avaliacaoRepository;
            _filmeRepository = filmeRepository;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        public async Task<OperationResult<PageResult<AvaliacaoResponseDto>>> ObterPorFilmePaginadoAsync(
            int filmeId, PagedRequestDto request, string? baseUrl = null)
        {
            _logger.LogInformation("Buscando avaliações paginadas para o filme ID {FilmeId}", filmeId);

            var filme = await _filmeRepository.ObterPorIdAsync(filmeId);
            if (filme == null)
            {
                return OperationResult<PageResult<AvaliacaoResponseDto>>.NotFound($"Filme com ID {filmeId} não existe.");
            }

            var pagedEntities = await _avaliacaoRepository.ObterPorFilmeIdPaginadoAsync(filmeId, request.PageNumber, request.PageSize);
            var dtos = pagedEntities.Items.Select(a => a.ToResponseDto()).ToList();

            var pagedResult = new PageResult<AvaliacaoResponseDto>(
                dtos, pagedEntities.TotalItems, pagedEntities.PageNumber, pagedEntities.PageSize);

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _hateoasService.EnriquecerListaAvaliacoes(pagedResult, baseUrl, filmeId);
            }

            return OperationResult<PageResult<AvaliacaoResponseDto>>.Success(
                pagedResult, $"Avaliações recuperadas com sucesso para o filme ID {filmeId}.");
        }

        public async Task<OperationResult<AvaliacaoResponseDto>> ObterPorIdAsync(int id, string? baseUrl = null)
        {
            _logger.LogInformation("Buscando avaliação por ID {Id}", id);

            var avaliacao = await _avaliacaoRepository.ObterPorIdAsync(id);
            if (avaliacao == null)
            {
                return OperationResult<AvaliacaoResponseDto>.NotFound($"Avaliação com ID {id} não encontrada.");
            }

            var dto = avaliacao.ToResponseDto();
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _hateoasService.EnriquecerAvaliacao(dto, baseUrl);
            }

            return OperationResult<AvaliacaoResponseDto>.Success(dto);
        }

        public async Task<OperationResult<AvaliacaoResponseDto>> CriarAsync(AvaliacaoRequestDto dto, string? baseUrl = null)
        {
            _logger.LogInformation("Adicionando nova avaliação para o filme ID {FilmeId} por {Usuario}", dto.FilmeId, dto.Usuario);

            try
            {
                var filme = await _filmeRepository.ObterPorIdAsync(dto.FilmeId);
                if (filme == null)
                {
                    return OperationResult<AvaliacaoResponseDto>.NotFound($"Não é possível avaliar: Filme com ID {dto.FilmeId} não existe.");
                }

                var avaliacao = dto.ToEntity();
                var resultadoRepo = await _avaliacaoRepository.AdicionarAsync(avaliacao);

                if (!resultadoRepo.IsSuccess || resultadoRepo.Data == null)
                {
                    return OperationResult<AvaliacaoResponseDto>.Failure(resultadoRepo.Message, resultadoRepo.StatusCode, resultadoRepo.Errors.ToArray());
                }

                var responseDto = resultadoRepo.Data.ToResponseDto();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    _hateoasService.EnriquecerAvaliacao(responseDto, baseUrl);
                }

                return OperationResult<AvaliacaoResponseDto>.Created(responseDto, "Avaliação registrada com sucesso.");
            }
            catch (DomainValidationException ex)
            {
                return OperationResult<AvaliacaoResponseDto>.Failure(ex.Message, 400, ex.Erros.ToArray());
            }
        }

        public async Task<OperationResult> RemoverAsync(int id)
        {
            _logger.LogInformation("Removendo avaliação ID {Id}", id);

            try
            {
                await _avaliacaoRepository.RemoverAsync(id);
                return OperationResult.Success("Avaliação removida com sucesso.");
            }
            catch (EntityNotFoundException ex)
            {
                return OperationResult.NotFound(ex.Message);
            }
        }
    }
}
