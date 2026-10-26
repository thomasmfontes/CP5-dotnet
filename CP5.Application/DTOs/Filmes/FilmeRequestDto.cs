using System.ComponentModel.DataAnnotations;

namespace CP5.Application.DTOs.Filmes
{
    public class FilmeRequestDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "O título deve ter entre 1 e 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A sinopse não pode exceder 1000 caracteres.")]
        public string Sinopse { get; set; } = string.Empty;

        [Required(ErrorMessage = "O gênero é obrigatório.")]
        [StringLength(60, ErrorMessage = "O gênero não pode exceder 60 caracteres.")]
        public string Genero { get; set; } = string.Empty;

        [Range(1888, 2035, ErrorMessage = "O ano de lançamento deve ser válido.")]
        public int AnoLancamento { get; set; }

        [Range(0.0, 10.0, ErrorMessage = "A nota IMDb deve estar entre 0.0 e 10.0.")]
        public decimal NotaImdb { get; set; }

        [Range(1, 600, ErrorMessage = "A duração deve ser entre 1 e 600 minutos.")]
        public int DuracaoMinutos { get; set; }

        [Url(ErrorMessage = "A URL do poster deve ser válida.")]
        public string? PosterUrl { get; set; }
    }
}
