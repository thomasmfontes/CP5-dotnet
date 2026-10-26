using CP5.Domain.Common;
using Xunit;

namespace CP5.UnitTests.Common
{
    public class OperationResultTests
    {
        [Fact]
        public void Success_DeveConfigurarIsSuccessTrueEStatusCode200()
        {
            var result = OperationResult.Success("Ok");

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Ok", result.Message);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_DeveConfigurarIsSuccessFalseEErros()
        {
            var result = OperationResult.Failure("Falha", 400, "Campo X inválido", "Campo Y obrigatório");

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void NotFound_DeveRetornarStatusCode404()
        {
            var result = OperationResult<string>.NotFound("Registro não encontrado");

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public void PageResult_DeveCalcularTotalPagesEFlagsDePaginacaoCorretamente()
        {
            var items = new List<string> { "Item 1", "Item 2", "Item 3" };
            var paged = new PageResult<string>(items, totalItems: 25, pageNumber: 2, pageSize: 10);

            Assert.Equal(2, paged.PageNumber);
            Assert.Equal(10, paged.PageSize);
            Assert.Equal(25, paged.TotalItems);
            Assert.Equal(3, paged.TotalPages); // 25 / 10 = ceil(2.5) = 3
            Assert.True(paged.HasPreviousPage);
            Assert.True(paged.HasNextPage);
        }
    }
}
