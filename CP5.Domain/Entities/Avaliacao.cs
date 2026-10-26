using CP5.Domain.Exceptions;

namespace CP5.Domain.Entities
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int FilmeId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public int Nota { get; set; }
        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;

        public Filme? Filme { get; set; }

        public Avaliacao() { }

        public Avaliacao(int filmeId, string usuario, string comentario, int nota)
        {
            FilmeId = filmeId;
            Usuario = usuario;
            Comentario = comentario;
            Nota = nota;
            DataAvaliacao = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            var erros = new List<string>();

            if (FilmeId <= 0)
                erros.Add("O identificador do filme é obrigatório.");

            if (string.IsNullOrWhiteSpace(Usuario))
                erros.Add("O nome do usuário é obrigatório.");
            else if (Usuario.Length > 80)
                erros.Add("O nome do usuário não pode exceder 80 caracteres.");

            if (string.IsNullOrWhiteSpace(Comentario))
                erros.Add("O comentário é obrigatório.");
            else if (Comentario.Length > 500)
                erros.Add("O comentário não pode exceder 500 caracteres.");

            if (Nota < 1 || Nota > 5)
                erros.Add("A nota da avaliação deve estar entre 1 e 5 estrelas.");

            if (erros.Count > 0)
                throw new DomainValidationException("Falha na validação da entidade Avaliação.", erros);
        }
    }
}
