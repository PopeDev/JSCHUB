using JSCHUB.Components;
using JSCHUB.Application.Interfaces;
using JSCHUB.Application.Services;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.BackgroundServices;
using JSCHUB.Infrastructure.Data;
using JSCHUB.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor
builder.Services.AddMudServices();

// Add DbContext
builder.Services.AddDbContext<ReminderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IReminderItemRepository, ReminderItemRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();

// Add Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IGastoService, GastoService>();

// Add Authentication Services (Singleton para mantener sesión global)
builder.Services.AddSingleton<JSCHUB.Infrastructure.Services.AuthService>();
builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());
builder.Services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());

// Add Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Background Services
builder.Services.AddHostedService<AlertGeneratorService>();

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();
    await db.Database.MigrateAsync();
    await SeedDataAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Seed data method
static async Task SeedDataAsync(ReminderDbContext db)
{
    var now = DateTime.UtcNow;
    
    // Seed Personas (solo si no existen)
    if (!await db.Personas.AnyAsync())
    {
        var personas = new[]
        {
            new JSCHUB.Domain.Entities.Persona { Id = Guid.NewGuid(), Nombre = "Pope", Activo = true },
            new JSCHUB.Domain.Entities.Persona { Id = Guid.NewGuid(), Nombre = "Javi", Activo = true },
            new JSCHUB.Domain.Entities.Persona { Id = Guid.NewGuid(), Nombre = "Carlos", Activo = true }
        };
        await db.Personas.AddRangeAsync(personas);
        await db.SaveChangesAsync();
        
        // Seed Gastos de ejemplo
        var pope = personas[0];
        var javi = personas[1];
        var carlos = personas[2];
        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);
        
        var gastos = new[]
        {
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Almuerzo equipo",
                Notas = "Comida de trabajo en restaurante",
                Importe = 45.50m,
                Moneda = "EUR",
                PagadoPorId = pope.Id,
                FechaPago = today,
                HoraPago = new TimeOnly(13, 30),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Registrado
            },
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Licencia software",
                Notas = "Renovación anual JetBrains",
                Importe = 120.00m,
                Moneda = "EUR",
                PagadoPorId = javi.Id,
                FechaPago = today.AddDays(-3),
                HoraPago = new TimeOnly(10, 15),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Registrado
            },
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Material oficina",
                Notas = "Folios, bolígrafos, post-its",
                Importe = 32.80m,
                Moneda = "EUR",
                PagadoPorId = carlos.Id,
                FechaPago = today.AddDays(-7),
                HoraPago = new TimeOnly(17, 45),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Registrado
            },
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Taxi aeropuerto",
                Notas = "Viaje a reunión Madrid",
                Importe = 28.00m,
                Moneda = "EUR",
                PagadoPorId = pope.Id,
                FechaPago = today.AddDays(-2),
                HoraPago = new TimeOnly(7, 30),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Registrado
            },
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Cena cliente",
                Notas = "Cena con cliente potencial",
                Importe = 85.00m,
                Moneda = "EUR",
                PagadoPorId = javi.Id,
                FechaPago = today.AddDays(-1),
                HoraPago = new TimeOnly(21, 0),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Registrado
            }
        };
        await db.Gastos.AddRangeAsync(gastos);
        await db.SaveChangesAsync();
    }
    
    // Seed ReminderItems (solo si no existen)
    if (!await db.ReminderItems.AnyAsync())
    {
        var items = new[]
        {
            new JSCHUB.Domain.Entities.ReminderItem
            {
                Id = Guid.NewGuid(),
                Title = "Renovación dominio ejemplo.es",
                Description = "Renovar el dominio ejemplo.es en Nominalia",
                Category = JSCHUB.Domain.Enums.Category.Renewal,
                Tags = ["dominio", "nominalia"],
                Status = JSCHUB.Domain.Enums.ItemStatus.Active,
                ScheduleType = JSCHUB.Domain.Enums.ScheduleType.Recurring,
                DueAt = now.AddMonths(2),
                RecurrenceFrequency = JSCHUB.Domain.Enums.RecurrenceFrequency.Yearly,
                LeadTimeDays = [30, 7],
                NextOccurrenceAt = now.AddMonths(2),
                Metadata = new Dictionary<string, string>
                {
                    ["domainName"] = "ejemplo.es",
                    ["provider"] = "Nominalia",
                    ["registrar"] = "Nominalia Internet S.L."
                },
                CreatedAt = now,
                UpdatedAt = now
            },
            new JSCHUB.Domain.Entities.ReminderItem
            {
                Id = Guid.NewGuid(),
                Title = "Pago VPS Hetzner",
                Description = "Renovación mensual del servidor VPS en Hetzner",
                Category = JSCHUB.Domain.Enums.Category.Renewal,
                Tags = ["hosting", "vps", "hetzner"],
                Status = JSCHUB.Domain.Enums.ItemStatus.Active,
                ScheduleType = JSCHUB.Domain.Enums.ScheduleType.Recurring,
                DueAt = now.AddDays(15),
                RecurrenceFrequency = JSCHUB.Domain.Enums.RecurrenceFrequency.Monthly,
                LeadTimeDays = [7, 3],
                NextOccurrenceAt = now.AddDays(15),
                Metadata = new Dictionary<string, string>
                {
                    ["provider"] = "Hetzner",
                    ["plan"] = "CX21",
                    ["price"] = "5.83€/mes"
                },
                CreatedAt = now,
                UpdatedAt = now
            },
            new JSCHUB.Domain.Entities.ReminderItem
            {
                Id = Guid.NewGuid(),
                Title = "Declaración IVA Trimestral",
                Description = "Modelo 303 - Declaración trimestral del IVA",
                Category = JSCHUB.Domain.Enums.Category.Reminder,
                Tags = ["impuestos", "iva", "hacienda"],
                Status = JSCHUB.Domain.Enums.ItemStatus.Active,
                ScheduleType = JSCHUB.Domain.Enums.ScheduleType.Recurring,
                DueAt = new DateTime(now.Year, ((now.Month - 1) / 3 + 1) * 3 + 1, 20),
                RecurrenceFrequency = JSCHUB.Domain.Enums.RecurrenceFrequency.Quarterly,
                LeadTimeDays = [15, 7, 3],
                NextOccurrenceAt = new DateTime(now.Year, ((now.Month - 1) / 3 + 1) * 3 + 1, 20),
                Metadata = new Dictionary<string, string>
                {
                    ["modelo"] = "303",
                    ["organismo"] = "AEAT",
                    ["periodicidad"] = "Trimestral"
                },
                CreatedAt = now,
                UpdatedAt = now
            },
            new JSCHUB.Domain.Entities.ReminderItem
            {
                Id = Guid.NewGuid(),
                Title = "Certificado SSL Let's Encrypt",
                Description = "Renovación automática del certificado SSL (verificar que funciona)",
                Category = JSCHUB.Domain.Enums.Category.Reminder,
                Tags = ["ssl", "seguridad", "letsencrypt"],
                Status = JSCHUB.Domain.Enums.ItemStatus.Active,
                ScheduleType = JSCHUB.Domain.Enums.ScheduleType.Recurring,
                DueAt = now.AddDays(60),
                RecurrenceFrequency = JSCHUB.Domain.Enums.RecurrenceFrequency.Quarterly,
                LeadTimeDays = [14],
                NextOccurrenceAt = now.AddDays(60),
                Metadata = new Dictionary<string, string>
                {
                    ["domain"] = "*.ejemplo.com",
                    ["provider"] = "Let's Encrypt"
                },
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await db.ReminderItems.AddRangeAsync(items);
        await db.SaveChangesAsync();
    }
}
