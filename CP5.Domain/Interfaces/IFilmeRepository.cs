using CP5.Domain.Common;
using CP5.Domain.Entities;

namespace CP5.Domain.Interfaces
{
    public interface IFilmeRepository
    {
        Task<PageResult<Filme>> ObterTodosPaginadoAsync(int pageNumber, int pageSize, string? genero = null);
        Task<Filme?> ObterPorIdAsync(int id);
        Task<bool> ExistePorTituloEAnoAsync(string titulo, int anoLancamento, int? ignorarId = null);

        // Abordagem 1: Retorno de Erro no Repository (OperationResult)
        Task<OperationResult<Filme>> AdicionarComResultadoAsync(Filme filme);

        // Abordagem 2: Lançamento de Erros Customizados no Repository (Exceptions)
        Task<Filme> BuscarComErroLancadoAsync(int id);
        Task AtualizarComLancamentoAsync(Filme filme);
        Task RemoverComLancamentoAsync(int id);
    }
}
