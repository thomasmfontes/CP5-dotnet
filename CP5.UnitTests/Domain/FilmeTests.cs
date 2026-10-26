using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using Xunit;

namespace CP5.UnitTests.Domain
{
    public class FilmeTests
    {
        [Fact]
        public void CriarFilme_ComDadosValidos_DeveInstanciarComSucesso()
        {
            // Act
            var filme = new Filme("Interestelar", "Sinopse do filme", "Ficção Científica", 2014, 8.7m, 169);

            // Assert
            Assert.NotNull(filme);
            Assert.Equal("Interestelar", filme.Titulo);
            Assert.Equal("Ficção Científica", filme.Genero);
            Assert.Equal(2014, filme.AnoLancamento);
            Assert.Equal(8.7m, filme.NotaImdb);
            Assert.Equal(169, filme.DuracaoMinutos);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CriarFilme_ComTituloInvalido_DeveLancarDomainValidationException(string? tituloInvalido)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainValidationException>(() =>
                new Filme(tituloInvalido!, "Sinopse", "Drama", 2020, 8.0m, 120));

            Assert.Contains(ex.Erros, e => e.Contains("título"));
        }

        [Theory]
        [InlineData(1800)]
        [InlineData(2150)]
        public void CriarFilme_ComAnoInvalido_DeveLancarDomainValidationException(int anoInvalido)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainValidationException>(() =>
                new Filme("Filme Teste", "Sinopse", "Ação", anoInvalido, 7.5m, 110));

            Assert.Contains(ex.Erros, e => e.Contains("ano"));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(10.5)]
        public void CriarFilme_ComNotaImdbForaDaFaixa_DeveLancarDomainValidationException(decimal notaInvalida)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainValidationException>(() =>
                new Filme("Filme Teste", "Sinopse", "Comédia", 2021, notaInvalida, 90));

            Assert.Contains(ex.Erros, e => e.Contains("IMDb"));
        }

        [Fact]
        public void AtualizarDados_ComDadosValidos_DeveAtualizarPropriedadesEData()
        {
            // Arrange
            var filme = new Filme("Titulo Antigo", "Sinopse Antiga", "Ação", 2010, 7.0m, 100);

            // Act
            filme.AtualizarDados("Titulo Novo", "Sinopse Nova", "Drama", 2012, 8.5m, 130, "http://poster.jpg");

            // Assert
            Assert.Equal("Titulo Novo", filme.Titulo);
            Assert.Equal("Sinopse Nova", filme.Sinopse);
            Assert.Equal("Drama", filme.Genero);
            Assert.Equal(2012, filme.AnoLancamento);
            Assert.Equal(8.5m, filme.NotaImdb);
            Assert.NotNull(filme.DataAtualizacao);
        }
    }
}
