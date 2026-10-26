using CP5.Domain.Common;

namespace CP5.Application.DTOs.Filmes
{
    public class FilmeResponseDto : ResourceWithLinks
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Sinopse { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int AnoLancamento { get; set; }
        public decimal NotaImdb { get; set; }
        public int DuracaoMinutos { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public int TotalAvaliacoes { get; set; }
        public decimal? MediaAvaliacoes { get; set; }
    }
}
