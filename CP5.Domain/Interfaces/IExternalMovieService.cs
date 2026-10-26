using CP5.Domain.Common;

namespace CP5.Domain.Interfaces
{
    public interface IExternalMovieService
    {
        Task<OperationResult<ExternalMovieDetails>> ObterDetalhesExternosAsync(string titulo, CancellationToken cancellationToken = default);
    }
}
