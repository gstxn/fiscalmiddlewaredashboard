using System;
using FiscalMiddleware.Domain.Enums;
using FiscalMiddleware.Domain.Exceptions;

namespace FiscalMiddleware.Domain.Entities;

public class Transacao
{
    public Guid Id { get; private set; }
    public Guid LoteId { get; private set; }
    public string DocumentoFiscal { get; private set; }
    public string CnpjEmitente { get; private set; }
    public decimal Valor { get; private set; }
    public TipoOperacao TipoOperacao { get; private set; }
    public string PayloadOriginal { get; private set; }
    public string ChaveIdempotencia { get; private set; }
    public StatusTransacao Status { get; private set; }
    
    // Navigation property for EF Core
    public Lote Lote { get; private set; }
    
    public void AlterarStatus(StatusTransacao novoStatus)
    {
        Status = novoStatus;
    }

    protected Transacao() { } // EF Core

    public Transacao(Guid loteId, string documentoFiscal, string cnpjEmitente, decimal valor, TipoOperacao tipoOperacao, string payloadOriginal)
    {
        Id = Guid.NewGuid();
        LoteId = loteId;
        DocumentoFiscal = documentoFiscal ?? throw new DomainException("Documento Fiscal é obrigatório");
        CnpjEmitente = cnpjEmitente ?? throw new DomainException("CNPJ do Emitente é obrigatório");
        
        if (valor <= 0)
            throw new DomainException("Valor deve ser maior que zero");
        Valor = valor;
        
        TipoOperacao = tipoOperacao;
        PayloadOriginal = payloadOriginal ?? throw new DomainException("Payload original é obrigatório");
        
        Status = StatusTransacao.Pendente;
        ChaveIdempotencia = GerarChaveIdempotencia(cnpjEmitente, documentoFiscal, valor);
    }

    private string GerarChaveIdempotencia(string cnpj, string documento, decimal valor)
    {
        // Hash de CNPJ + número do documento + valor
        var input = $"{cnpj}-{documento}-{valor}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
