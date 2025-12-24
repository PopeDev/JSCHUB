using JSCHUB.Application.Storage.DTOs;
using JSCHUB.Application.Storage.Exceptions;
using JSCHUB.Application.Storage.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JSCHUB.Infrastructure.Api;

/// <summary>
/// Endpoints de la API para gestión de ficheros.
/// </summary>
public static class FilesEndpoints
{
    /// <summary>
    /// Registra los endpoints de la API de ficheros.
    /// </summary>
    public static void MapFilesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/files")
            .WithTags("Files");

        // GET /api/files?path=... - Listar items
        group.MapGet("", ListFiles)
            .WithName("ListFiles")
            .WithDescription("Lista archivos y carpetas en una ruta");

        // POST /api/files/folders - Crear carpeta
        group.MapPost("/folders", CreateFolder)
            .WithName("CreateFolder")
            .WithDescription("Crea una nueva carpeta");

        // POST /api/files/upload?path=... - Subir archivo
        group.MapPost("/upload", UploadFile)
            .WithName("UploadFile")
            .WithDescription("Sube un archivo a la carpeta especificada")
            .DisableAntiforgery();

        // GET /api/files/download?path=... - Descargar archivo
        group.MapGet("/download", DownloadFile)
            .WithName("DownloadFile")
            .WithDescription("Descarga un archivo");

        // GET /api/files/thumbnail?path=...&size=... - Obtener miniatura
        group.MapGet("/thumbnail", GetThumbnail)
            .WithName("GetThumbnail")
            .WithDescription("Obtiene la miniatura de una imagen");

        // DELETE /api/files?path=...&recursive=... - Eliminar
        group.MapDelete("", DeleteItem)
            .WithName("DeleteFile")
            .WithDescription("Elimina un archivo o carpeta");

        // POST /api/files/rename - Renombrar
        group.MapPost("/rename", RenameItem)
            .WithName("RenameFile")
            .WithDescription("Renombra un archivo o carpeta");

        // POST /api/files/move - Mover
        group.MapPost("/move", MoveItem)
            .WithName("MoveFile")
            .WithDescription("Mueve un archivo o carpeta");

        // GET /api/files/info?path=... - Info de un item
        group.MapGet("/info", GetItemInfo)
            .WithName("GetFileInfo")
            .WithDescription("Obtiene información de un archivo o carpeta");
    }

    private static async Task<IResult> ListFiles(
        [FromQuery] string? path,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            var items = await storage.ListAsync(path, ct);
            return Results.Ok(items);
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "La ruta especificada no existe." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> CreateFolder(
        [FromBody] CreateFolderRequest request,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            await storage.CreateFolderAsync(request.Path, ct);
            return Results.Created($"/api/files?path={Uri.EscapeDataString(request.Path)}", new { path = request.Path });
        }
        catch (StorageItemExistsException)
        {
            return Results.Conflict(new { error = "Ya existe un item con ese nombre." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (InvalidFileNameException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> UploadFile(
        [FromQuery] string? path,
        IFormFile file,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "No se ha proporcionado un archivo." });
            }

            await using var stream = file.OpenReadStream();
            await storage.UploadAsync(path ?? "", file.FileName, stream, file.ContentType, ct);

            var filePath = string.IsNullOrEmpty(path) ? file.FileName : $"{path}/{file.FileName}";
            return Results.Created($"/api/files/download?path={Uri.EscapeDataString(filePath)}", new { path = filePath });
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "La carpeta destino no existe." });
        }
        catch (StorageItemExistsException)
        {
            return Results.Conflict(new { error = "Ya existe un archivo con ese nombre." });
        }
        catch (FileSizeExceededException ex)
        {
            return Results.StatusCode(413); // Payload Too Large
        }
        catch (ExtensionNotAllowedException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (InvalidFileNameException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> DownloadFile(
        [FromQuery] string path,
        [FromQuery] bool? inline,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Results.BadRequest(new { error = "Debe especificar la ruta del archivo." });
            }

            var (stream, contentType, fileName) = await storage.OpenReadAsync(path, ct);

            var disposition = inline == true ? "inline" : "attachment";
            return Results.Stream(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "El archivo no existe." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetThumbnail(
        [FromQuery] string path,
        [FromQuery] int? size,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Results.BadRequest(new { error = "Debe especificar la ruta del archivo." });
            }

            var thumbnailSize = size ?? 128;
            if (thumbnailSize < 32 || thumbnailSize > 512)
            {
                return Results.BadRequest(new { error = "El tamaño debe estar entre 32 y 512 píxeles." });
            }

            var result = await storage.GetThumbnailAsync(path, thumbnailSize, ct);

            if (result == null)
            {
                return Results.NotFound(new { error = "No se puede generar miniatura para este archivo." });
            }

            return Results.Stream(result.Data, result.ContentType, enableRangeProcessing: false);
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "El archivo no existe." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> DeleteItem(
        [FromQuery] string path,
        [FromQuery] bool? recursive,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Results.BadRequest(new { error = "Debe especificar la ruta del item." });
            }

            await storage.DeleteAsync(path, recursive ?? false, ct);
            return Results.NoContent();
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "El item no existe." });
        }
        catch (DirectoryNotEmptyException)
        {
            return Results.BadRequest(new { error = "La carpeta no está vacía. Use recursive=true para eliminar todo el contenido." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> RenameItem(
        [FromBody] RenameRequest request,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            await storage.RenameAsync(request.Path, request.NewName, ct);
            return Results.Ok(new { success = true });
        }
        catch (StorageItemNotFoundException)
        {
            return Results.NotFound(new { error = "El item no existe." });
        }
        catch (StorageItemExistsException)
        {
            return Results.Conflict(new { error = "Ya existe un item con ese nombre." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (InvalidFileNameException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> MoveItem(
        [FromBody] MoveRequest request,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            await storage.MoveAsync(request.SourcePath, request.DestinationFolderPath, ct);
            return Results.Ok(new { success = true });
        }
        catch (StorageItemNotFoundException ex)
        {
            return Results.NotFound(new { error = "El item o la carpeta destino no existe." });
        }
        catch (StorageItemExistsException)
        {
            return Results.Conflict(new { error = "Ya existe un item con ese nombre en el destino." });
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetItemInfo(
        [FromQuery] string path,
        IFileStorage storage,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Results.BadRequest(new { error = "Debe especificar la ruta del item." });
            }

            var item = await storage.GetItemAsync(path, ct);

            if (item == null)
            {
                return Results.NotFound(new { error = "El item no existe." });
            }

            return Results.Ok(item);
        }
        catch (PathTraversalException)
        {
            return Results.StatusCode(403);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
