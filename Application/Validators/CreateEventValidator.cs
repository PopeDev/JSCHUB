using FluentValidation;
using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Validators;

public class CreateEventValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.StartUtc)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria");

        RuleFor(x => x.EndUtc)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria")
            .GreaterThan(x => x.StartUtc).WithMessage("La fecha de fin debe ser posterior a la de inicio");

        RuleFor(x => x.MeetingUrl)
            .Must(BeAValidUrl).WithMessage("La URL de reunión debe ser válida")
            .When(x => !string.IsNullOrEmpty(x.MeetingUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateEventValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");

        RuleFor(x => x.StartUtc)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria");

        RuleFor(x => x.EndUtc)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria")
            .GreaterThan(x => x.StartUtc).WithMessage("La fecha de fin debe ser posterior a la de inicio");

        RuleFor(x => x.MeetingUrl)
            .Must(BeAValidUrl).WithMessage("La URL de reunión debe ser válida")
            .When(x => !string.IsNullOrEmpty(x.MeetingUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
