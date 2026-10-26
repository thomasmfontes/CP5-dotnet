using CP5.Domain.Common;
using CP5.Domain.Entities;

namespace CP5.Domain.Interfaces
{
    public interface IAvaliacaoRepository
    {
        Task<PageResult<Avaliacao>> ObterPorFilmeIdPaginadoAsync(int filmeId, int pageNumber, int pageSize);
        Task<Avaliacao?> ObterPorIdAsync(int id);
        Task<OperationResult<Avaliacao>> AdicionarAsync(Avaliacao avaliacao);
        Task RemoverAsync(int id);
    }
}
