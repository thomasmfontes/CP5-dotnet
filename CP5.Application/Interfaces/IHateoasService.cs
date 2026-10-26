using CP5.Application.DTOs.Avaliacoes;
using CP5.Application.DTOs.Filmes;
using CP5.Domain.Common;

namespace CP5.Application.Interfaces
{
    public interface IHateoasService
    {
        FilmeResponseDto EnriquecerFilme(FilmeResponseDto filme, string baseUrl);
        PageResult<FilmeResponseDto> EnriquecerListaFilmes(PageResult<FilmeResponseDto> pageResult, string baseUrl, string? genero = null);
        AvaliacaoResponseDto EnriquecerAvaliacao(AvaliacaoResponseDto avaliacao, string baseUrl);
        PageResult<AvaliacaoResponseDto> EnriquecerListaAvaliacoes(PageResult<AvaliacaoResponseDto> pageResult, string baseUrl, int filmeId);
    }
}
