using CP5.Application.DTOs.Filmes;
using CP5.Application.Services;
using CP5.Domain.Common;
using Xunit;

namespace CP5.UnitTests.Services
{
    public class HateoasServiceTests
    {
        private readonly HateoasService _hateoasService = new();

        [Fact]
        public void EnriquecerFilme_DeveGerarLinksHateoasCorretos()
        {
            // Arrange
            var filmeDto = new FilmeResponseDto { Id = 5, Titulo = "Filme Teste" };
            var baseUrl = "https://api.filmes.com";

            // Act
            _hateoasService.EnriquecerFilme(filmeDto, baseUrl);

            // Assert
            Assert.NotEmpty(filmeDto.Links);
            Assert.Contains(filmeDto.Links, l => l.Rel == "self" && l.Href == "https://api.filmes.com/api/filmes/5" && l.Method == "GET");
            Assert.Contains(filmeDto.Links, l => l.Rel == "update" && l.Method == "PUT");
            Assert.Contains(filmeDto.Links, l => l.Rel == "delete" && l.Method == "DELETE");
            Assert.Contains(filmeDto.Links, l => l.Rel == "filme-avaliacoes" && l.Method == "GET");
            Assert.Contains(filmeDto.Links, l => l.Rel == "external-info" && l.Method == "GET");
        }

        [Fact]
        public void EnriquecerListaFilmes_DeveEnriquecerCadaFilmeDaPagina()
        {
            // Arrange
            var filmes = new List<FilmeResponseDto>
            {
                new FilmeResponseDto { Id = 1 },
                new FilmeResponseDto { Id = 2 }
            };
            var paged = new PageResult<FilmeResponseDto>(filmes, 2, 1, 10);

            // Act
            _hateoasService.EnriquecerListaFilmes(paged, "https://api.filmes.com");

            // Assert
            Assert.All(paged.Items, item => Assert.NotEmpty(item.Links));
        }
    }
}
