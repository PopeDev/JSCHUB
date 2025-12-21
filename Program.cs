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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();
builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();
builder.Services.AddScoped<IEnlaceProyectoRepository, EnlaceProyectoRepository>();
builder.Services.AddScoped<IRecursoProyectoRepository, RecursoProyectoRepository>();

// Add Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IEnlaceProyectoService, EnlaceProyectoService>();
builder.Services.AddScoped<IRecursoProyectoService, RecursoProyectoService>();

// Add Authentication Services (Scoped para aislamiento de sesión por circuito)
builder.Services.AddScoped<JSCHUB.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());
builder.Services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());

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

    // Seed Usuarios (solo si no existen)
    if (!await db.Usuarios.AnyAsync())
    {
        var usuarios = new[]
        {
            new JSCHUB.Domain.Entities.Usuario { Id = Guid.NewGuid(), Nombre = "Pope", Activo = true },
            new JSCHUB.Domain.Entities.Usuario { Id = Guid.NewGuid(), Nombre = "Javi", Activo = true },
            new JSCHUB.Domain.Entities.Usuario { Id = Guid.NewGuid(), Nombre = "Carlos", Activo = true }
        };
        await db.Usuarios.AddRangeAsync(usuarios);
        await db.SaveChangesAsync();

        // Seed Gastos de ejemplo
        var pope = usuarios[0];
        var javi = usuarios[1];
        var carlos = usuarios[2];
        var today = DateOnly.FromDateTime(DateTime.Now);

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
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Pagado
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
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Pagado
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
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Saldado
            },
            new JSCHUB.Domain.Entities.Gasto
            {
                Id = Guid.NewGuid(),
                Concepto = "Taxi aeropuerto",
                Notas = "Viaje a reunión Madrid",
                Importe = 28.00m,
                Moneda = "EUR",
                PagadoPorId = pope.Id,
                FechaPago = today.AddDays(5),
                HoraPago = new TimeOnly(7, 30),
                Estado = JSCHUB.Domain.Enums.EstadoGasto.Previsto
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
                Estado = JSCHUB.Domain.Enums.EstadoGasto.PendienteDevolucion
            }
        };
        await db.Gastos.AddRangeAsync(gastos);
        await db.SaveChangesAsync();
    }

    // Seed Proyectos (solo si no existen)
    if (!await db.Proyectos.AnyAsync())
    {
        var proyecto = new JSCHUB.Domain.Entities.Proyecto
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente X - Web corporativa",
            Descripcion = "Desarrollo y mantenimiento de la web corporativa del Cliente X",
            Estado = JSCHUB.Domain.Enums.EstadoProyecto.Activo,
            EnlacePrincipal = "https://github.com/example/cliente-x-web",
            Etiquetas = "cliente, web, producción",
            CreadoPor = "sistema",
            CreadoEl = now,
            ModificadoPor = "sistema",
            ModificadoEl = now
        };
        await db.Proyectos.AddAsync(proyecto);
        await db.SaveChangesAsync();

        // Seed Enlaces del proyecto
        var enlaces = new[]
        {
            new JSCHUB.Domain.Entities.EnlaceProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Titulo = "Repositorio",
                Url = "https://github.com/example/cliente-x-web",
                Descripcion = "Repositorio principal del proyecto",
                Tipo = JSCHUB.Domain.Enums.TipoEnlace.Repositorio,
                Orden = 1,
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.EnlaceProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Titulo = "Producción",
                Url = "https://www.clientex.com",
                Descripcion = "Sitio web en producción",
                Tipo = JSCHUB.Domain.Enums.TipoEnlace.Entorno,
                Orden = 2,
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.EnlaceProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Titulo = "Preproducción",
                Url = "https://staging.clientex.com",
                Descripcion = "Entorno de staging",
                Tipo = JSCHUB.Domain.Enums.TipoEnlace.Entorno,
                Orden = 3,
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.EnlaceProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Titulo = "Panel Hosting",
                Url = "https://panel.hosting.com/clientex",
                Descripcion = "Panel de administración del hosting",
                Tipo = JSCHUB.Domain.Enums.TipoEnlace.Panel,
                Orden = 4,
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.EnlaceProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Titulo = "Figma",
                Url = "https://figma.com/file/xxx",
                Descripcion = "Diseños del proyecto",
                Tipo = JSCHUB.Domain.Enums.TipoEnlace.Diseno,
                Orden = 5,
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            }
        };
        await db.EnlacesProyecto.AddRangeAsync(enlaces);

        // Seed Recursos del proyecto
        var recursos = new[]
        {
            new JSCHUB.Domain.Entities.RecursoProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Nombre = "Checklist despliegue",
                Tipo = JSCHUB.Domain.Enums.TipoRecurso.Nota,
                Contenido = "1. Ejecutar tests\n2. Build de producción\n3. Subir a staging\n4. Verificar en staging\n5. Deploy a producción\n6. Verificar en producción\n7. Notificar al cliente",
                Etiquetas = "despliegue, producción",
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.RecursoProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Nombre = "Credenciales entregadas",
                Tipo = JSCHUB.Domain.Enums.TipoRecurso.Nota,
                Contenido = "Al cliente se le han entregado:\n- Acceso al panel de hosting\n- Credenciales de admin del CMS\n- Acceso FTP (deshabilitado por defecto)",
                Etiquetas = "credenciales, cliente",
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            },
            new JSCHUB.Domain.Entities.RecursoProyecto
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyecto.Id,
                Nombre = "Guía DNS",
                Tipo = JSCHUB.Domain.Enums.TipoRecurso.Enlace,
                Url = "https://docs.example.com/dns-guide",
                Etiquetas = "dns, documentación",
                CreadoPor = "sistema",
                CreadoEl = now,
                ModificadoPor = "sistema",
                ModificadoEl = now
            }
        };
        await db.RecursosProyecto.AddRangeAsync(recursos);
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
                DueAt = GetNextQuarterlyDueDate(now),
                RecurrenceFrequency = JSCHUB.Domain.Enums.RecurrenceFrequency.Quarterly,
                LeadTimeDays = [15, 7, 3],
                NextOccurrenceAt = GetNextQuarterlyDueDate(now),
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

static DateTime GetNextQuarterlyDueDate(DateTime now)
{
    var currentQuarter = (now.Month - 1) / 3 + 1;
    var targetMonth = currentQuarter * 3 + 1;
    var targetYear = now.Year;
    
    if (targetMonth > 12)
    {
        targetMonth = 1;
        targetYear++;
    }
    
    return new DateTime(targetYear, targetMonth, 20, 0, 0, 0, DateTimeKind.Utc);
}
