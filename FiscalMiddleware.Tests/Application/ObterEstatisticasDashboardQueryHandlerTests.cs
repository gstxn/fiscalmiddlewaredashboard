using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Application.Queries;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FiscalMiddleware.Tests.Application;

public class ObterEstatisticasDashboardQueryHandlerTests
{
    private readonly IDashboardRepository _repositoryMock;
    private readonly ObterEstatisticasDashboardQueryHandler _handler;

    public ObterEstatisticasDashboardQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IDashboardRepository>();
        _handler = new ObterEstatisticasDashboardQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Estatisticas_Do_Repositorio()
    {
        // Arrange
        var query = new ObterEstatisticasDashboardQuery();
        var expectedStats = new DashboardStatsDto
        {
            MensagensPendentes = 10,
            MensagensProcessando = 5,
            TaxaSucesso24h = "95.0%",
            TaxaFalha24h = "5.0%",
            DlqCount = 2
        };

        _repositoryMock.ObterEstatisticasAsync(Arg.Any<CancellationToken>())
            .Returns(expectedStats);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MensagensPendentes.Should().Be(10);
        result.MensagensProcessando.Should().Be(5);
        result.TaxaSucesso24h.Should().Be("95.0%");
        result.TaxaFalha24h.Should().Be("5.0%");
        result.DlqCount.Should().Be(2);

        await _repositoryMock.Received(1).ObterEstatisticasAsync(Arg.Any<CancellationToken>());
    }
}
