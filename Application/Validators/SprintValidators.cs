using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateSprintValidator : AbstractValidator<CreateSprintDto>
{
    public CreateSprintValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty().WithMessage("El ProyectoId es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Temporizacion)
            .NotEmpty().WithMessage("La temporalización es obligatoria")
            .MaximumLength(200).WithMessage("La temporalización no puede exceder 200 caracteres");

        RuleFor(x => x.Objetivo)
            .MaximumLength(1000).WithMessage("El objetivo no puede exceder 1000 caracteres");

        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria");

        RuleFor(x => x.FechaFin)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior o igual a la fecha de inicio");
    }
}

public class UpdateSprintValidator : AbstractValidator<UpdateSprintDto>
{
    public UpdateSprintValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Temporizacion)
            .NotEmpty().WithMessage("La temporalización es obligatoria")
            .MaximumLength(200).WithMessage("La temporalización no puede exceder 200 caracteres");

        RuleFor(x => x.Objetivo)
            .MaximumLength(1000).WithMessage("El objetivo no puede exceder 1000 caracteres");

        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria");

        RuleFor(x => x.FechaFin)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior o igual a la fecha de inicio");
    }
}
