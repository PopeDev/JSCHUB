using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class UsuarioProyectoService : IUsuarioProyectoService
{
    private readonly IUsuarioProyectoRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ILogger<UsuarioProyectoService> _logger;

    public UsuarioProyectoService(
        IUsuarioProyectoRepository repository,
        IUsuarioRepository usuarioRepository,
        IProyectoRepository proyectoRepository,
        ILogger<UsuarioProyectoService> logger)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _proyectoRepository = proyectoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProyectoAsignadoDto>> GetProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var asignaciones = await _repository.GetProyectosByUsuarioAsync(usuarioId, ct);
        return asignaciones.Select(a => new ProyectoAsignadoDto(
            a.ProyectoId,
            a.Proyecto.Nombre,
            a.Proyecto.EsGeneral,
            a.Rol
        ));
    }

    public async Task<IEnumerable<UsuarioAsignadoDto>> GetUsuariosDelProyectoAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var asignaciones = await _repository.GetUsuariosByProyectoAsync(proyectoId, ct);
        return asignaciones.Select(a => new UsuarioAsignadoDto(
            a.UsuarioId,
            a.Usuario.Nombre,
            a.Rol,
            a.FechaAsignacion
        ));
    }

    public async Task<bool> TieneAccesoAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        return await _repository.TieneAccesoAsync(usuarioId, proyectoId, ct);
    }

    public async Task<RolProyecto?> GetRolAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        return await _repository.GetRolAsync(usuarioId, proyectoId, ct);
    }

    public async Task<bool> TieneAccesoATodosAsync(Guid usuarioId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default)
    {
        return await _repository.TieneAccesoATodosAsync(usuarioId, proyectoIds, ct);
    }

    public async Task<bool> PuedeRealizarAccionAsync(Guid usuarioId, Guid proyectoId, AccionProyecto accion, CancellationToken ct = default)
    {
        var rol = await _repository.GetRolAsync(usuarioId, proyectoId, ct);
        if (rol == null) return false;

        return accion switch
        {
            AccionProyecto.Ver => true, // Todos los roles pueden ver
            AccionProyecto.CrearItem => rol != RolProyecto.Viewer,
            AccionProyecto.EditarItemPropio => rol != RolProyecto.Viewer,
            AccionProyecto.EditarItemOtros => rol == RolProyecto.Admin,
            AccionProyecto.EliminarItemPropio => rol != RolProyecto.Viewer,
            AccionProyecto.EliminarItemOtros => rol == RolProyecto.Admin,
            AccionProyecto.GestionarProyecto => rol == RolProyecto.Admin,
            AccionProyecto.GestionarUsuarios => rol == RolProyecto.Admin,
            AccionProyecto.GestionarKanban => rol != RolProyecto.Viewer,
            _ => false
        };
    }

    public async Task<ProyectoSimpleDto?> GetProyectoGeneralAsync(CancellationToken ct = default)
    {
        var proyecto = await _repository.GetProyectoGeneralAsync(ct);
        if (proyecto == null) return null;

        return new ProyectoSimpleDto(proyecto.Id, proyecto.Nombre, proyecto.EsGeneral);
    }

    public async Task<UsuarioProyectoDto> AsignarUsuarioAsync(AsignarUsuarioProyectoDto dto, string asignadoPor, CancellationToken ct = default)
    {
        // Validar que el usuario existe y está activo
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId, ct)
            ?? throw new KeyNotFoundException($"Usuario {dto.UsuarioId} no encontrado");

        if (!usuario.Activo)
            throw new InvalidOperationException($"El usuario {usuario.Nombre} no está activo");

        // Validar que el proyecto existe
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"Proyecto {dto.ProyectoId} no encontrado");

        // Verificar que no exista la asignación
        var existente = await _repository.GetAsync(dto.UsuarioId, dto.ProyectoId, ct);
        if (existente != null)
            throw new InvalidOperationException($"El usuario {usuario.Nombre} ya está asignado al proyecto {proyecto.Nombre}");

        var asignacion = new UsuarioProyecto
        {
            UsuarioId = dto.UsuarioId,
            ProyectoId = dto.ProyectoId,
            Rol = dto.Rol,
            FechaAsignacion = DateTime.UtcNow,
            AsignadoPor = asignadoPor
        };

        await _repository.AddAsync(asignacion, ct);

        _logger.LogInformation("Usuario {UsuarioId} asignado al proyecto {ProyectoId} con rol {Rol}",
            dto.UsuarioId, dto.ProyectoId, dto.Rol);

        return new UsuarioProyectoDto(
            usuario.Id,
            usuario.Nombre,
            proyecto.Id,
            proyecto.Nombre,
            proyecto.EsGeneral,
            asignacion.Rol,
            asignacion.FechaAsignacion,
            asignacion.AsignadoPor
        );
    }

    public async Task<UsuarioProyectoDto> ActualizarRolAsync(ActualizarRolUsuarioProyectoDto dto, CancellationToken ct = default)
    {
        var asignacion = await _repository.GetAsync(dto.UsuarioId, dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"Asignación no encontrada para usuario {dto.UsuarioId} en proyecto {dto.ProyectoId}");

        asignacion.Rol = dto.NuevoRol;
        await _repository.UpdateAsync(asignacion, ct);

        _logger.LogInformation("Rol actualizado para usuario {UsuarioId} en proyecto {ProyectoId}: {NuevoRol}",
            dto.UsuarioId, dto.ProyectoId, dto.NuevoRol);

        return new UsuarioProyectoDto(
            asignacion.Usuario.Id,
            asignacion.Usuario.Nombre,
            asignacion.Proyecto.Id,
            asignacion.Proyecto.Nombre,
            asignacion.Proyecto.EsGeneral,
            asignacion.Rol,
            asignacion.FechaAsignacion,
            asignacion.AsignadoPor
        );
    }

    public async Task DesasignarUsuarioAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        var asignacion = await _repository.GetAsync(usuarioId, proyectoId, ct)
            ?? throw new KeyNotFoundException($"Asignación no encontrada");

        // Verificar que el proyecto no sea el Proyecto General (solo se puede desasignar si no quedan más usuarios admins)
        if (asignacion.Proyecto.EsGeneral)
        {
            var adminsEnGeneral = await _repository.GetUsuariosByProyectoAsync(proyectoId, ct);
            var otrosAdmins = adminsEnGeneral.Count(a => a.Rol == RolProyecto.Admin && a.UsuarioId != usuarioId);
            if (otrosAdmins == 0)
                throw new InvalidOperationException("No se puede desasignar al último Admin del Proyecto General");
        }

        // Verificar que no tenga gastos pendientes en el proyecto
        var gastosNoSaldados = await _repository.CountGastosNoSaldadosAsync(usuarioId, proyectoId, ct);
        if (gastosNoSaldados > 0)
            throw new InvalidOperationException($"El usuario tiene {gastosNoSaldados} gasto(s) no saldados en este proyecto. Debe saldarlos antes de desasignar.");

        await _repository.DeleteAsync(usuarioId, proyectoId, ct);

        _logger.LogInformation("Usuario {UsuarioId} desasignado del proyecto {ProyectoId}",
            usuarioId, proyectoId);
    }
}
