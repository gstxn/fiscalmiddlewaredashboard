using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FiscalMiddleware.Domain.Entities;

namespace FiscalMiddleware.Infrastructure.Data.Configurations;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("Lotes");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Origem)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(x => x.Transacoes)
            .WithOne(x => x.Lote)
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("Transacoes");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.DocumentoFiscal)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.CnpjEmitente)
            .IsRequired()
            .HasMaxLength(14);
            
        builder.Property(x => x.Valor)
            .HasColumnType("numeric(18,2)");
            
        builder.Property(x => x.TipoOperacao)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.PayloadOriginal)
            .IsRequired()
            .HasColumnType("jsonb"); // Using jsonb for Postgres
            
        builder.Property(x => x.ChaveIdempotencia)
            .IsRequired()
            .HasMaxLength(100);
            
        // Index on idempotency key
        builder.HasIndex(x => x.ChaveIdempotencia);
    }
}

public class HistoricoProcessamentoConfiguration : IEntityTypeConfiguration<HistoricoProcessamento>
{
    public void Configure(EntityTypeBuilder<HistoricoProcessamento> builder)
    {
        builder.ToTable("HistoricosProcessamento");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.StatusAlcancado)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.MotivoFalha)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.DetalheErro)
            .HasMaxLength(2048);
            
        builder.HasOne(x => x.Transacao)
            .WithMany()
            .HasForeignKey(x => x.TransacaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
