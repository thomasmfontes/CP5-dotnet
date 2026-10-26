using CP5.Application.DTOs.Avaliacoes;
using CP5.Application.DTOs.Filmes;
using CP5.Application.Interfaces;
using CP5.Domain.Common;

namespace CP5.Application.Services
{
    public class HateoasService : IHateoasService
    {
        public FilmeResponseDto EnriquecerFilme(FilmeResponseDto filme, string baseUrl)
        {
            var baseEndpoint = $"{baseUrl.TrimEnd('/')}/api/filmes";
            filme.Links.Clear();

            filme.Links.Add(new LinkDto("self", $"{baseEndpoint}/{filme.Id}", "GET"));
            filme.Links.Add(new LinkDto("update", $"{baseEndpoint}/{filme.Id}", "PUT"));
            filme.Links.Add(new LinkDto("delete", $"{baseEndpoint}/{filme.Id}", "DELETE"));
            filme.Links.Add(new LinkDto("filme-avaliacoes", $"{baseUrl.TrimEnd('/')}/api/avaliacoes/filme/{filme.Id}", "GET"));
            filme.Links.Add(new LinkDto("external-info", $"{baseEndpoint}/{filme.Id}/external-info", "GET"));

            return filme;
        }

        public PageResult<FilmeResponseDto> EnriquecerListaFilmes(PageResult<FilmeResponseDto> pageResult, string baseUrl, string? genero = null)
        {
            var baseEndpoint = $"{baseUrl.TrimEnd('/')}/api/filmes";
            var generoParam = !string.IsNullOrWhiteSpace(genero) ? $"&genero={Uri.EscapeDataString(genero)}" : string.Empty;

            foreach (var item in pageResult.Items)
            {
                EnriquecerFilme(item, baseUrl);
            }

            return pageResult;
        }

        public AvaliacaoResponseDto EnriquecerAvaliacao(AvaliacaoResponseDto avaliacao, string baseUrl)
        {
            var baseEndpoint = $"{baseUrl.TrimEnd('/')}/api/avaliacoes";
            avaliacao.Links.Clear();

            avaliacao.Links.Add(new LinkDto("self", $"{baseEndpoint}/{avaliacao.Id}", "GET"));
            avaliacao.Links.Add(new LinkDto("delete", $"{baseEndpoint}/{avaliacao.Id}", "DELETE"));
            avaliacao.Links.Add(new LinkDto("filme", $"{baseUrl.TrimEnd('/')}/api/filmes/{avaliacao.FilmeId}", "GET"));

            return avaliacao;
        }

        public PageResult<AvaliacaoResponseDto> EnriquecerListaAvaliacoes(PageResult<AvaliacaoResponseDto> pageResult, string baseUrl, int filmeId)
        {
            foreach (var item in pageResult.Items)
            {
                EnriquecerAvaliacao(item, baseUrl);
            }

            return pageResult;
        }
    }
}
