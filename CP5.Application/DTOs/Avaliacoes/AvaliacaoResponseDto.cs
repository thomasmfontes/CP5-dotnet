using CP5.Domain.Common;

namespace CP5.Application.DTOs.Avaliacoes
{
    public class AvaliacaoResponseDto : ResourceWithLinks
    {
        public int Id { get; set; }
        public int FilmeId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public int Nota { get; set; }
        public DateTime DataAvaliacao { get; set; }
    }
}
