using Microsoft.EntityFrameworkCore;
using FiscalMiddleware.Domain.Entities;
using FiscalMiddleware.Infrastructure.Data.Configurations;

namespace FiscalMiddleware.Infrastructure.Data;

public class FiscalDbContext : DbContext
{
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<HistoricoProcessamento> Historicos { get; set; }

    public FiscalDbContext(DbContextOptions<FiscalDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiscalDbContext).Assembly);
    }
}
