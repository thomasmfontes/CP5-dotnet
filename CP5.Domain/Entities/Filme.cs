using CP5.Domain.Exceptions;

namespace CP5.Domain.Entities
{
    public class Filme
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Sinopse { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int AnoLancamento { get; set; }
        public decimal NotaImdb { get; set; }
        public int DuracaoMinutos { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

        public Filme() { }

        public Filme(string titulo, string sinopse, string genero, int anoLancamento, decimal notaImdb, int duracaoMinutos, string? posterUrl = null)
        {
            Titulo = titulo;
            Sinopse = sinopse;
            Genero = genero;
            AnoLancamento = anoLancamento;
            NotaImdb = notaImdb;
            DuracaoMinutos = duracaoMinutos;
            PosterUrl = posterUrl;
            DataCadastro = DateTime.UtcNow;
            Validar();
        }

        public void AtualizarDados(string titulo, string sinopse, string genero, int anoLancamento, decimal notaImdb, int duracaoMinutos, string? posterUrl)
        {
            Titulo = titulo;
            Sinopse = sinopse;
            Genero = genero;
            AnoLancamento = anoLancamento;
            NotaImdb = notaImdb;
            DuracaoMinutos = duracaoMinutos;
            PosterUrl = posterUrl;
            DataAtualizacao = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(Titulo))
                erros.Add("O título do filme é obrigatório.");
            else if (Titulo.Length > 150)
                erros.Add("O título do filme não pode exceder 150 caracteres.");

            if (string.IsNullOrWhiteSpace(Genero))
                erros.Add("O gênero do filme é obrigatório.");
            else if (Genero.Length > 60)
                erros.Add("O gênero não pode exceder 60 caracteres.");

            if (AnoLancamento < 1888 || AnoLancamento > DateTime.UtcNow.Year + 5)
                erros.Add($"O ano de lançamento deve estar entre 1888 e {DateTime.UtcNow.Year + 5}.");

            if (NotaImdb < 0 || NotaImdb > 10)
                erros.Add("A nota IMDb deve estar entre 0.0 e 10.0.");

            if (DuracaoMinutos <= 0 || DuracaoMinutos > 600)
                erros.Add("A duração deve ser maior que 0 e até 600 minutos.");

            if (erros.Count > 0)
                throw new DomainValidationException("Falha na validação da entidade Filme.", erros);
        }
    }
}
