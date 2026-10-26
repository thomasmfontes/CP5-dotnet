using CP5.Application.DTOs.Common;
using CP5.Application.DTOs.Filmes;
using CP5.Domain.Common;

namespace CP5.Application.Interfaces
{
    public interface IFilmeService
    {
        Task<OperationResult<PageResult<FilmeResponseDto>>> ObterPaginadoAsync(PagedRequestDto request, string? genero = null, string? baseUrl = null);
        Task<OperationResult<FilmeResponseDto>> ObterPorIdAsync(int id, string? baseUrl = null);
        Task<OperationResult<FilmeResponseDto>> CriarAsync(FilmeRequestDto dto, string? baseUrl = null);
        Task<OperationResult<FilmeResponseDto>> AtualizarAsync(int id, FilmeRequestDto dto, string? baseUrl = null);
        Task<OperationResult> RemoverAsync(int id);

        // Chamada externa com resiliência
        Task<OperationResult<ExternalMovieDetails>> ObterInfoExternaAsync(int id);

        // Demonstração dos requisitos específicos de Erros Customizados:
        Task<OperationResult<FilmeResponseDto>> BuscarComErroLancadoAsync(int id);
        Task<OperationResult<FilmeResponseDto>> CriarComRetornoErroRepositoryAsync(FilmeRequestDto dto);
    }
}
