using CP5.Application.DTOs.Avaliacoes;
using CP5.Application.DTOs.Common;
using CP5.Domain.Common;

namespace CP5.Application.Interfaces
{
    public interface IAvaliacaoService
    {
        Task<OperationResult<PageResult<AvaliacaoResponseDto>>> ObterPorFilmePaginadoAsync(int filmeId, PagedRequestDto request, string? baseUrl = null);
        Task<OperationResult<AvaliacaoResponseDto>> ObterPorIdAsync(int id, string? baseUrl = null);
        Task<OperationResult<AvaliacaoResponseDto>> CriarAsync(AvaliacaoRequestDto dto, string? baseUrl = null);
        Task<OperationResult> RemoverAsync(int id);
    }
}
