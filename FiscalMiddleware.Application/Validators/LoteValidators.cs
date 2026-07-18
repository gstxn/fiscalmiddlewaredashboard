using FluentValidation;
using FiscalMiddleware.Application.DTOs;

namespace FiscalMiddleware.Application.Validators;

public class TransacaoDtoValidator : AbstractValidator<TransacaoDto>
{
    public TransacaoDtoValidator()
    {
        RuleFor(x => x.DocumentoFiscal).NotEmpty().WithMessage("Documento Fiscal é obrigatório.");
        RuleFor(x => x.CnpjEmitente).NotEmpty().Length(14).WithMessage("CNPJ Emitente deve conter 14 caracteres.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
        RuleFor(x => x.TipoOperacao).IsInEnum().WithMessage("Tipo de Operação inválido.");
        RuleFor(x => x.PayloadOriginal).NotNull().WithMessage("Payload original é obrigatório.");
    }
}

public class ReceberLoteCommandValidator : AbstractValidator<Commands.ReceberLoteCommand>
{
    public ReceberLoteCommandValidator()
    {
        RuleFor(x => x.Lote.Origem).NotEmpty().WithMessage("Origem é obrigatória.");
        RuleFor(x => x.Lote.Transacoes)
            .NotEmpty().WithMessage("Lote deve conter pelo menos uma transação.")
            .Must(t => t.Count <= 1000).WithMessage("Lote não pode exceder 1000 transações.");
            
        RuleForEach(x => x.Lote.Transacoes).SetValidator(new TransacaoDtoValidator());
    }
}
