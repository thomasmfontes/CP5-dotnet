using CP5.Application.DTOs.Filmes;
using CP5.Domain.Entities;

namespace CP5.Application.Mappers
{
    public static class FilmeMapper
    {
        public static Filme ToEntity(this FilmeRequestDto dto)
        {
            return new Filme(
                dto.Titulo,
                dto.Sinopse,
                dto.Genero,
                dto.AnoLancamento,
                dto.NotaImdb,
                dto.DuracaoMinutos,
                dto.PosterUrl
            );
        }

        public static FilmeResponseDto ToResponseDto(this Filme entity)
        {
            var totalAvaliacoes = entity.Avaliacoes?.Count ?? 0;
            decimal? mediaAvaliacoes = totalAvaliacoes > 0
                ? Math.Round((decimal)entity.Avaliacoes!.Average(a => a.Nota), 1)
                : null;

            return new FilmeResponseDto
            {
                Id = entity.Id,
                Titulo = entity.Titulo,
                Sinopse = entity.Sinopse,
                Genero = entity.Genero,
                AnoLancamento = entity.AnoLancamento,
                NotaImdb = entity.NotaImdb,
                DuracaoMinutos = entity.DuracaoMinutos,
                PosterUrl = entity.PosterUrl,
                DataCadastro = entity.DataCadastro,
                DataAtualizacao = entity.DataAtualizacao,
                TotalAvaliacoes = totalAvaliacoes,
                MediaAvaliacoes = mediaAvaliacoes
            };
        }
    }
}
