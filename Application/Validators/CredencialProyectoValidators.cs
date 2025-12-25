using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateCredencialProyectoValidator : AbstractValidator<CreateCredencialProyectoDto>
{
    public CreateCredencialProyectoValidator()
    {
        RuleFor(x => x.EnlaceProyectoId)
            .NotEmpty()
            .WithMessage("El enlace del proyecto es obligatorio");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la credencial es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede superar los 100 caracteres");

        RuleFor(x => x.Usuario)
            .NotEmpty()
            .WithMessage("El nombre de usuario es obligatorio")
            .MaximumLength(500)
            .WithMessage("El usuario no puede superar los 500 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria")
            .MaximumLength(1000)
            .WithMessage("La contraseña no puede superar los 1000 caracteres");

        RuleFor(x => x.Notas)
            .MaximumLength(2000)
            .WithMessage("Las notas no pueden superar los 2000 caracteres");
    }
}

public class UpdateCredencialProyectoValidator : AbstractValidator<UpdateCredencialProyectoDto>
{
    public UpdateCredencialProyectoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la credencial es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede superar los 100 caracteres");

        RuleFor(x => x.Usuario)
            .NotEmpty()
            .WithMessage("El nombre de usuario es obligatorio")
            .MaximumLength(500)
            .WithMessage("El usuario no puede superar los 500 caracteres");

        RuleFor(x => x.Password)
            .MaximumLength(1000)
            .WithMessage("La contraseña no puede superar los 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Notas)
            .MaximumLength(2000)
            .WithMessage("Las notas no pueden superar los 2000 caracteres");
    }
}
