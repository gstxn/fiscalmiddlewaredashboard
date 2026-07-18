using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Application.Queries;
using FiscalMiddleware.WebAPI.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace FiscalMiddleware.Tests.WebAPI.Controllers;

public class DashboardControllerTests
{
    private readonly IMediator _mediatorMock;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _controller = new DashboardController(_mediatorMock);
    }

    [Fact]
    public async Task ObterEstatisticas_Deve_Retornar_Ok_Com_Dados()
    {
        // Arrange
        var expectedStats = new DashboardStatsDto
        {
            MensagensPendentes = 15,
            MensagensProcessando = 2,
            TaxaSucesso24h = "99.0%",
            TaxaFalha24h = "1.0%",
            DlqCount = 0
        };

        _mediatorMock.Send(Arg.Any<ObterEstatisticasDashboardQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedStats);

        // Act
        var result = await _controller.ObterEstatisticas(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<DashboardStatsDto>().Subject;

        returnedStats.MensagensPendentes.Should().Be(15);
        returnedStats.DlqCount.Should().Be(0);

        await _mediatorMock.Received(1).Send(Arg.Any<ObterEstatisticasDashboardQuery>(), Arg.Any<CancellationToken>());
    }
}
