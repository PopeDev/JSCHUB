using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateKanbanColumnValidator : AbstractValidator<CreateKanbanColumnDto>
{
    public CreateKanbanColumnValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty().WithMessage("El proyecto es obligatorio");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(100).WithMessage("El título no puede superar 100 caracteres");

        RuleFor(x => x.Posicion)
            .GreaterThanOrEqualTo(0).WithMessage("La posición debe ser mayor o igual a 0")
            .When(x => x.Posicion.HasValue);
    }
}

public class UpdateKanbanColumnValidator : AbstractValidator<UpdateKanbanColumnDto>
{
    public UpdateKanbanColumnValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(100).WithMessage("El título no puede superar 100 caracteres");
    }
}

public class CreateKanbanTaskValidator : AbstractValidator<CreateKanbanTaskDto>
{
    public CreateKanbanTaskValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty().WithMessage("El proyecto es obligatorio");

        RuleFor(x => x.ColumnaId)
            .NotEmpty().WithMessage("La columna es obligatoria");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Prioridad)
            .IsInEnum().WithMessage("La prioridad no es válida")
            .When(x => x.Prioridad.HasValue);

        RuleFor(x => x.HorasEstimadas)
            .GreaterThanOrEqualTo(0).WithMessage("Las horas estimadas deben ser mayor o igual a 0")
            .When(x => x.HorasEstimadas.HasValue);

        RuleFor(x => x.Posicion)
            .GreaterThanOrEqualTo(0).WithMessage("La posición debe ser mayor o igual a 0")
            .When(x => x.Posicion.HasValue);
    }
}

public class UpdateKanbanTaskValidator : AbstractValidator<UpdateKanbanTaskDto>
{
    public UpdateKanbanTaskValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Prioridad)
            .IsInEnum().WithMessage("La prioridad no es válida");

        RuleFor(x => x.HorasEstimadas)
            .GreaterThanOrEqualTo(0).WithMessage("Las horas estimadas deben ser mayor o igual a 0");
    }
}

public class MoveKanbanTaskValidator : AbstractValidator<MoveKanbanTaskDto>
{
    public MoveKanbanTaskValidator()
    {
        RuleFor(x => x.ColumnaDestinoId)
            .NotEmpty().WithMessage("La columna destino es obligatoria");

        RuleFor(x => x.NuevaPosicion)
            .GreaterThanOrEqualTo(0).WithMessage("La posición debe ser mayor o igual a 0");
    }
}
