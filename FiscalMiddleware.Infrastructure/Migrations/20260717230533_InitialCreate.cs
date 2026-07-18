using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalMiddleware.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentoFiscal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CnpjEmitente = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TipoOperacao = table.Column<string>(type: "text", nullable: false),
                    PayloadOriginal = table.Column<string>(type: "jsonb", nullable: false),
                    ChaveIdempotencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosProcessamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tentativa = table.Column<int>(type: "integer", nullable: false),
                    StatusAlcancado = table.Column<string>(type: "text", nullable: false),
                    MotivoFalha = table.Column<string>(type: "text", nullable: false),
                    UltimoStatusHttp = table.Column<int>(type: "integer", nullable: true),
                    DetalheErro = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DuracaoMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosProcessamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosProcessamento_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosProcessamento_TransacaoId",
                table: "HistoricosProcessamento",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ChaveIdempotencia",
                table: "Transacoes",
                column: "ChaveIdempotencia");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_LoteId",
                table: "Transacoes",
                column: "LoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosProcessamento");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "Lotes");
        }
    }
}
