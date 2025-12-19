using FluentValidation;
using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Validators;

public class CreateRecursoProyectoValidator : AbstractValidator<CreateRecursoProyectoDto>
{
    public CreateRecursoProyectoValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty().WithMessage("El proyecto es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de recurso no es válido");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL es obligatoria para recursos de tipo Enlace o Documento externo")
            .Must(BeAValidUrl).WithMessage("La URL debe ser válida")
            .MaximumLength(2000).WithMessage("La URL no puede superar 2000 caracteres")
            .When(x => x.Tipo == TipoRecurso.Enlace || x.Tipo == TipoRecurso.DocumentoExterno);

        RuleFor(x => x.Contenido)
            .NotEmpty().WithMessage("El contenido es obligatorio para recursos de tipo Nota")
            .When(x => x.Tipo == TipoRecurso.Nota);

        RuleFor(x => x.Contenido)
            .MaximumLength(10000).WithMessage("El contenido no puede superar 10000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Contenido));

        RuleFor(x => x.Etiquetas)
            .MaximumLength(500).WithMessage("Las etiquetas no pueden superar 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Etiquetas));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateRecursoProyectoValidator : AbstractValidator<UpdateRecursoProyectoDto>
{
    public UpdateRecursoProyectoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de recurso no es válido");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL es obligatoria para recursos de tipo Enlace o Documento externo")
            .Must(BeAValidUrl).WithMessage("La URL debe ser válida")
            .MaximumLength(2000).WithMessage("La URL no puede superar 2000 caracteres")
            .When(x => x.Tipo == TipoRecurso.Enlace || x.Tipo == TipoRecurso.DocumentoExterno);

        RuleFor(x => x.Contenido)
            .NotEmpty().WithMessage("El contenido es obligatorio para recursos de tipo Nota")
            .When(x => x.Tipo == TipoRecurso.Nota);

        RuleFor(x => x.Contenido)
            .MaximumLength(10000).WithMessage("El contenido no puede superar 10000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Contenido));

        RuleFor(x => x.Etiquetas)
            .MaximumLength(500).WithMessage("Las etiquetas no pueden superar 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Etiquetas));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
