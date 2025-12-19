using FluentValidation;
using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Validators;

public class CreateProyectoValidator : AbstractValidator<CreateProyectoDto>
{
    public CreateProyectoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.EnlacePrincipal)
            .Must(BeAValidUrl).WithMessage("El enlace principal debe ser una URL válida")
            .When(x => !string.IsNullOrEmpty(x.EnlacePrincipal));

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

public class UpdateProyectoValidator : AbstractValidator<UpdateProyectoDto>
{
    public UpdateProyectoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Estado)
            .IsInEnum().WithMessage("El estado no es válido");

        RuleFor(x => x.EnlacePrincipal)
            .Must(BeAValidUrl).WithMessage("El enlace principal debe ser una URL válida")
            .When(x => !string.IsNullOrEmpty(x.EnlacePrincipal));

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
