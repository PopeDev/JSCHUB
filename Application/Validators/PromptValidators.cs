using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreatePromptValidator : AbstractValidator<CreatePromptDto>
{
    public CreatePromptValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("El usuario creador es obligatorio");

        RuleFor(x => x.ToolId)
            .NotEmpty().WithMessage("La herramienta es obligatoria");
    }
}

public class UpdatePromptValidator : AbstractValidator<UpdatePromptDto>
{
    public UpdatePromptValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria");

        RuleFor(x => x.ToolId)
            .NotEmpty().WithMessage("La herramienta es obligatoria");
    }
}
