using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateEnlaceProyectoValidator : AbstractValidator<CreateEnlaceProyectoDto>
{
    public CreateEnlaceProyectoValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty().WithMessage("El proyecto es obligatorio");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(100).WithMessage("El título no puede superar 100 caracteres");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL es obligatoria")
            .Must(BeAValidUrl).WithMessage("La URL debe ser válida")
            .MaximumLength(2000).WithMessage("La URL no puede superar 2000 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden debe ser mayor o igual a 0")
            .When(x => x.Orden.HasValue);
    }

    private static bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateEnlaceProyectoValidator : AbstractValidator<UpdateEnlaceProyectoDto>
{
    public UpdateEnlaceProyectoValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(100).WithMessage("El título no puede superar 100 caracteres");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL es obligatoria")
            .Must(BeAValidUrl).WithMessage("La URL debe ser válida")
            .MaximumLength(2000).WithMessage("La URL no puede superar 2000 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de enlace no es válido");

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden debe ser mayor o igual a 0");
    }

    private static bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
