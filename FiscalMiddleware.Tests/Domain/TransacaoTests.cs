using System;
using FiscalMiddleware.Domain.Entities;
using FiscalMiddleware.Domain.Enums;
using FiscalMiddleware.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FiscalMiddleware.Tests.Domain;

public class TransacaoTests
{
    [Fact]
    public void Deve_Criar_Transacao_Valida_E_Gerar_Chave_Idempotencia()
    {
        // Arrange
        var loteId = Guid.NewGuid();
        var documentoFiscal = "NFe-12345";
        var cnpj = "12345678901234";
        var valor = 1500.50m;
        var tipoOperacao = TipoOperacao.Emissao;
        var payload = "{\"key\":\"value\"}";

        // Act
        var transacao = new Transacao(loteId, documentoFiscal, cnpj, valor, tipoOperacao, payload);

        // Assert
        transacao.Id.Should().NotBeEmpty();
        transacao.LoteId.Should().Be(loteId);
        transacao.Status.Should().Be(StatusTransacao.Pendente);
        transacao.ChaveIdempotencia.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Nao_Deve_Criar_Transacao_Com_Valor_Invalido()
    {
        // Arrange & Act
        Action action = () => new Transacao(Guid.NewGuid(), "Doc", "CNPJ", 0, TipoOperacao.Emissao, "{}");

        // Assert
        action.Should().Throw<DomainException>().WithMessage("Valor deve ser maior que zero");
    }

    [Fact]
    public void Chaves_Idempotencia_Devem_Ser_Iguais_Para_Mesmo_Input()
    {
        // Arrange
        var loteId1 = Guid.NewGuid();
        var loteId2 = Guid.NewGuid();
        var documentoFiscal = "NFe-999";
        var cnpj = "99999999999999";
        var valor = 500m;

        // Act
        var t1 = new Transacao(loteId1, documentoFiscal, cnpj, valor, TipoOperacao.Emissao, "{}");
        var t2 = new Transacao(loteId2, documentoFiscal, cnpj, valor, TipoOperacao.Emissao, "{}");

        // Assert
        t1.ChaveIdempotencia.Should().Be(t2.ChaveIdempotencia);
    }
}
