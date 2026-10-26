using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using Xunit;

namespace CP5.UnitTests.Domain
{
    public class AvaliacaoTests
    {
        [Fact]
        public void CriarAvaliacao_ComDadosValidos_DeveInstanciarComSucesso()
        {
            // Act
            var avaliacao = new Avaliacao(1, "Thomas", "Filme espetacular!", 5);

            // Assert
            Assert.NotNull(avaliacao);
            Assert.Equal(1, avaliacao.FilmeId);
            Assert.Equal("Thomas", avaliacao.Usuario);
            Assert.Equal("Filme espetacular!", avaliacao.Comentario);
            Assert.Equal(5, avaliacao.Nota);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-2)]
        public void CriarAvaliacao_ComNotaInvalida_DeveLancarDomainValidationException(int notaInvalida)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainValidationException>(() =>
                new Avaliacao(1, "Usuario", "Comentario legal", notaInvalida));

            Assert.Contains(ex.Erros, e => e.Contains("nota"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CriarAvaliacao_ComUsuarioVazio_DeveLancarDomainValidationException(string? usuarioInvalido)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainValidationException>(() =>
                new Avaliacao(1, usuarioInvalido!, "Comentario valido", 4));

            Assert.Contains(ex.Erros, e => e.Contains("usuário"));
        }
    }
}
