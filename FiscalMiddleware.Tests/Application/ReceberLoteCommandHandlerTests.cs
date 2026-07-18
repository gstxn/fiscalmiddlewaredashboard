using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Application.Commands;
using FiscalMiddleware.Application.DTOs;
using FiscalMiddleware.Application.Handlers;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FiscalMiddleware.Tests.Application;

public class ReceberLoteCommandHandlerTests
{
    private readonly ILoteRepository _loteRepoMock;
    private readonly ITransacaoRepository _transacaoRepoMock;
    private readonly IMessagePublisher _publisherMock;
    private readonly IValidator<ReceberLoteCommand> _validatorMock;
    private readonly ILogger<ReceberLoteCommandHandler> _loggerMock;
    private readonly ReceberLoteCommandHandler _handler;

    public ReceberLoteCommandHandlerTests()
    {
        _loteRepoMock = Substitute.For<ILoteRepository>();
        _transacaoRepoMock = Substitute.For<ITransacaoRepository>();
        _publisherMock = Substitute.For<IMessagePublisher>();
        _validatorMock = Substitute.For<IValidator<ReceberLoteCommand>>();
        _loggerMock = Substitute.For<ILogger<ReceberLoteCommandHandler>>();

        _handler = new ReceberLoteCommandHandler(
            _loteRepoMock, _transacaoRepoMock, _publisherMock, _validatorMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Falha_Se_Lote_For_Invalido()
    {
        // Arrange
        var command = new ReceberLoteCommand(new LoteDto());
        var validationFailure = new ValidationResult(new[] { new ValidationFailure("Origem", "Erro") });
        _validatorMock.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(validationFailure);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Sucesso.Should().BeFalse();
        result.MensagemErro.Should().Contain("Erro");
        await _loteRepoMock.DidNotReceive().SalvarAsync(Arg.Any<Lote>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Salvar_E_Publicar_Se_Valido()
    {
        // Arrange
        var dto = new LoteDto
        {
            Origem = "SistemaA",
            Transacoes = new List<TransacaoDto>
            {
                new() { DocumentoFiscal = "123", CnpjEmitente = "12345678901234", Valor = 100, TipoOperacao = FiscalMiddleware.Domain.Enums.TipoOperacao.Emissao, PayloadOriginal = "{}" }
            }
        };
        var command = new ReceberLoteCommand(dto);
        _validatorMock.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Sucesso.Should().BeTrue();
        await _loteRepoMock.Received(1).SalvarAsync(Arg.Any<Lote>(), Arg.Any<CancellationToken>());
        await _publisherMock.Received(1).PublicarTransacaoAsync(Arg.Any<Transacao>(), Arg.Any<CancellationToken>());
    }
}
