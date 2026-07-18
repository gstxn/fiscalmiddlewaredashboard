using System;
using System.Collections.Generic;

namespace FiscalMiddleware.Domain.Entities;

public class Lote
{
    public Guid Id { get; private set; }
    public string Origem { get; private set; }
    public DateTime DataRecebimento { get; private set; }
    
    private readonly List<Transacao> _transacoes = new();
    public IReadOnlyCollection<Transacao> Transacoes => _transacoes.AsReadOnly();

    protected Lote() { } // EF Core

    public Lote(Guid id, string origem)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Origem = origem;
        DataRecebimento = DateTime.UtcNow;
    }

    public void AdicionarTransacao(Transacao transacao)
    {
        if (transacao == null) throw new ArgumentNullException(nameof(transacao));
        _transacoes.Add(transacao);
    }
}
