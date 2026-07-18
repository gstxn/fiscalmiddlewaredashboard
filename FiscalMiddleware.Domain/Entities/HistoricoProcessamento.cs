using System;
using FiscalMiddleware.Domain.Enums;

namespace FiscalMiddleware.Domain.Entities;

public class HistoricoProcessamento
{
    public Guid Id { get; private set; }
    public Guid TransacaoId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public int Tentativa { get; private set; }
    public StatusTransacao StatusAlcancado { get; private set; }
    public MotivoFalha MotivoFalha { get; private set; }
    public int? UltimoStatusHttp { get; private set; }
    public string DetalheErro { get; private set; } // Truncated to 2KB
    public long DuracaoMs { get; private set; }
    
    // Navigation
    public Transacao Transacao { get; private set; }

    protected HistoricoProcessamento() { }

    public HistoricoProcessamento(Guid transacaoId, int tentativa, StatusTransacao statusAlcancado, MotivoFalha motivoFalha, int? ultimoStatusHttp, string detalheErro, long duracaoMs)
    {
        Id = Guid.NewGuid();
        TransacaoId = transacaoId;
        Timestamp = DateTime.UtcNow;
        Tentativa = tentativa;
        StatusAlcancado = statusAlcancado;
        MotivoFalha = motivoFalha;
        UltimoStatusHttp = ultimoStatusHttp;
        
        // Truncate to 2KB if necessary
        DetalheErro = !string.IsNullOrEmpty(detalheErro) && detalheErro.Length > 2048 
            ? detalheErro.Substring(0, 2048) 
            : detalheErro;
            
        DuracaoMs = duracaoMs;
    }
}
