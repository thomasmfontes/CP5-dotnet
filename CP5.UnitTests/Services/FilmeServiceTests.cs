using CP5.Application.DTOs.Common;
using CP5.Application.DTOs.Filmes;
using CP5.Application.Interfaces;
using CP5.Application.Services;
using CP5.Domain.Common;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using CP5.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CP5.UnitTests.Services
{
    public class FilmeServiceTests
    {
        private readonly Mock<IFilmeRepository> _repositoryMock;
        private readonly Mock<IExternalMovieService> _externalServiceMock;
        private readonly Mock<IHateoasService> _hateoasServiceMock;
        private readonly Mock<ILogger<FilmeService>> _loggerMock;
        private readonly FilmeService _service;

        public FilmeServiceTests()
        {
            _repositoryMock = new Mock<IFilmeRepository>();
            _externalServiceMock = new Mock<IExternalMovieService>();
            _hateoasServiceMock = new Mock<IHateoasService>();
            _loggerMock = new Mock<ILogger<FilmeService>>();

            _service = new FilmeService(
                _repositoryMock.Object,
                _externalServiceMock.Object,
                _hateoasServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoFilmeExiste_DeveRetornarSuccess()
        {
            // Arrange
            var filme = new Filme("Interestelar", "Sinopse", "Ficção Científica", 2014, 8.7m, 169) { Id = 1 };
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(filme);

            // Act
            var result = await _service.ObterPorIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("Interestelar", result.Data!.Titulo);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoFilmeNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Filme?)null);

            // Act
            var result = await _service.ObterPorIdAsync(99);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task CriarAsync_QuandoFilmeJaExiste_DeveRetornarConflict()
        {
            // Arrange
            var dto = new FilmeRequestDto
            {
                Titulo = "Interestelar",
                Sinopse = "Sinopse",
                Genero = "Ficção Científica",
                AnoLancamento = 2014,
                NotaImdb = 8.7m,
                DuracaoMinutos = 169
            };

            _repositoryMock.Setup(r => r.ExistePorTituloEAnoAsync("Interestelar", 2014, null))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CriarAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task CriarAsync_QuandoDadosSaoValidos_DeveRetornarCreated()
        {
            // Arrange
            var dto = new FilmeRequestDto
            {
                Titulo = "Duna: Parte 2",
                Sinopse = "Paul Atreides se une a Chani e aos Fremen.",
                Genero = "Ficção Científica",
                AnoLancamento = 2024,
                NotaImdb = 8.6m,
                DuracaoMinutos = 166
            };

            _repositoryMock.Setup(r => r.ExistePorTituloEAnoAsync(dto.Titulo, dto.AnoLancamento, null))
                .ReturnsAsync(false);

            var filmeSalvo = new Filme(dto.Titulo, dto.Sinopse, dto.Genero, dto.AnoLancamento, dto.NotaImdb, dto.DuracaoMinutos) { Id = 10 };
            _repositoryMock.Setup(r => r.AdicionarComResultadoAsync(It.IsAny<Filme>()))
                .ReturnsAsync(OperationResult<Filme>.Created(filmeSalvo));

            // Act
            var result = await _service.CriarAsync(dto, "http://localhost:5000");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(10, result.Data!.Id);
            _hateoasServiceMock.Verify(h => h.EnriquecerFilme(It.IsAny<FilmeResponseDto>(), "http://localhost:5000"), Times.Once);
        }

        [Fact]
        public async Task ObterInfoExternaAsync_QuandoFilmeExiste_DeveChamarServicoExternoComResiliencia()
        {
            // Arrange
            var filme = new Filme("Interestelar", "Sinopse", "Ficção Científica", 2014, 8.7m, 169) { Id = 1 };
            _repositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(filme);

            var externalInfo = new ExternalMovieDetails
            {
                Title = "Interstellar",
                Language = "English",
                Rating = 8.7m,
                Summary = "A team of explorers travel through a wormhole in space."
            };

            _externalServiceMock.Setup(s => s.ObterDetalhesExternosAsync("Interestelar", default))
                .ReturnsAsync(OperationResult<ExternalMovieDetails>.Success(externalInfo));

            // Act
            var result = await _service.ObterInfoExternaAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Interstellar", result.Data!.Title);
            Assert.Equal("English", result.Data.Language);
        }

        [Fact]
        public async Task BuscarComErroLancadoAsync_QuandoFilmeNaoExiste_DevePropagarExceptionDoRepository()
        {
            // Arrange
            _repositoryMock.Setup(r => r.BuscarComErroLancadoAsync(999))
                .ThrowsAsync(new EntityNotFoundException(nameof(Filme), 999));

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.BuscarComErroLancadoAsync(999));
        }
    }
}
