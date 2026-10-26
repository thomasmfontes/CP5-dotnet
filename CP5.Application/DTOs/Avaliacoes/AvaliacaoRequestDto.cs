using System.ComponentModel.DataAnnotations;

namespace CP5.Application.DTOs.Avaliacoes
{
    public class AvaliacaoRequestDto
    {
        [Required(ErrorMessage = "O identificador do filme é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Identificador de filme inválido.")]
        public int FilmeId { get; set; }

        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome do usuário deve ter entre 2 e 80 caracteres.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O comentário é obrigatório.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "O comentário deve ter entre 3 e 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nota é obrigatória.")]
        [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5 estrelas.")]
        public int Nota { get; set; }
    }
}
