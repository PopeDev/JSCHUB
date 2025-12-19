using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateGastoValidator : AbstractValidator<CreateGastoDto>
{
    public CreateGastoValidator()
    {
        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto es obligatorio")
            .MaximumLength(200).WithMessage("El concepto no puede superar 200 caracteres");

        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor que 0");

        RuleFor(x => x.PagadoPorId)
            .NotEmpty().WithMessage("Debe indicar quién pagó");

        RuleFor(x => x.FechaPago)
            .NotEmpty().WithMessage("La fecha de pago es obligatoria");

        RuleFor(x => x.HoraPago)
            .NotEmpty().WithMessage("La hora de pago es obligatoria");

        RuleFor(x => x.Notas)
            .MaximumLength(1000).WithMessage("Las notas no pueden superar 1000 caracteres");
    }
}

public class UpdateGastoValidator : AbstractValidator<UpdateGastoDto>
{
    public UpdateGastoValidator()
    {
        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto es obligatorio")
            .MaximumLength(200).WithMessage("El concepto no puede superar 200 caracteres");

        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor que 0");

        RuleFor(x => x.PagadoPorId)
            .NotEmpty().WithMessage("Debe indicar quién pagó");

        RuleFor(x => x.FechaPago)
            .NotEmpty().WithMessage("La fecha de pago es obligatoria");

        RuleFor(x => x.HoraPago)
            .NotEmpty().WithMessage("La hora de pago es obligatoria");

        RuleFor(x => x.Notas)
            .MaximumLength(1000).WithMessage("Las notas no pueden superar 1000 caracteres");
    }
}

public class CreatePersonaValidator : AbstractValidator<CreatePersonaDto>
{
    public CreatePersonaValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El email no tiene un formato válido");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres");
    }
}
