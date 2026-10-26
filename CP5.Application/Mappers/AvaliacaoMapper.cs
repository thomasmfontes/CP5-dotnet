using CP5.Application.DTOs.Avaliacoes;
using CP5.Domain.Entities;

namespace CP5.Application.Mappers
{
    public static class AvaliacaoMapper
    {
        public static Avaliacao ToEntity(this AvaliacaoRequestDto dto)
        {
            return new Avaliacao(dto.FilmeId, dto.Usuario, dto.Comentario, dto.Nota);
        }

        public static AvaliacaoResponseDto ToResponseDto(this Avaliacao entity)
        {
            return new AvaliacaoResponseDto
            {
                Id = entity.Id,
                FilmeId = entity.FilmeId,
                Usuario = entity.Usuario,
                Comentario = entity.Comentario,
                Nota = entity.Nota,
                DataAvaliacao = entity.DataAvaliacao
            };
        }
    }
}
