using FluentValidation;
using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Validators;

public class CreateReminderItemValidator : AbstractValidator<CreateReminderItemDto>
{
    public CreateReminderItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");
        
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres");
        
        RuleFor(x => x.DueAt)
            .NotEmpty().When(x => x.ScheduleType == ScheduleType.OneTime)
            .WithMessage("La fecha de vencimiento es requerida para eventos únicos");
        
        RuleFor(x => x.RecurrenceFrequency)
            .NotEmpty().When(x => x.ScheduleType == ScheduleType.Recurring)
            .WithMessage("La frecuencia de recurrencia es requerida para eventos recurrentes");
        
        RuleFor(x => x.CustomIntervalDays)
            .GreaterThan(0).When(x => x.RecurrenceFrequency == RecurrenceFrequency.Custom)
            .WithMessage("El intervalo personalizado debe ser mayor a 0");
        
        RuleFor(x => x.LeadTimeDays)
            .Must(x => x == null || x.All(d => d >= 0))
            .WithMessage("Los días de antelación deben ser positivos");
    }
}

public class UpdateReminderItemValidator : AbstractValidator<UpdateReminderItemDto>
{
    public UpdateReminderItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres");
        
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres");
        
        RuleFor(x => x.DueAt)
            .NotEmpty().When(x => x.ScheduleType == ScheduleType.OneTime)
            .WithMessage("La fecha de vencimiento es requerida para eventos únicos");
        
        RuleFor(x => x.RecurrenceFrequency)
            .NotEmpty().When(x => x.ScheduleType == ScheduleType.Recurring)
            .WithMessage("La frecuencia de recurrencia es requerida para eventos recurrentes");
    }
}

public class SnoozeDtoValidator : AbstractValidator<SnoozeDto>
{
    public SnoozeDtoValidator()
    {
        RuleFor(x => x.SnoozedUntil)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha de posponer debe ser futura");
    }
}
