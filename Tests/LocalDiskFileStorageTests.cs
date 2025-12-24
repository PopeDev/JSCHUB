using JSCHUB.Application.Storage.DTOs;
using JSCHUB.Application.Storage.Exceptions;
using JSCHUB.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JSCHUB.Tests;

/// <summary>
/// Tests unitarios para LocalDiskFileStorage.
/// Para ejecutar: dotnet test
/// </summary>
public class LocalDiskFileStorageTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly LocalDiskFileStorage _storage;

    public LocalDiskFileStorageTests()
    {
        // Crear directorio temporal para tests
        _testRootPath = Path.Combine(Path.GetTempPath(), $"storage_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRootPath);

        var options = Options.Create(new StorageOptions
        {
            RootPath = _testRootPath,
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new List<string>(),
            EnableThumbnailCache = false
        });

        _storage = new LocalDiskFileStorage(options, NullLogger<LocalDiskFileStorage>.Instance);
    }

    public void Dispose()
    {
        // Limpiar directorio temporal
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
    }

    #region Path Traversal Tests

    [Fact]
    public async Task ListAsync_WithPathTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = "../../../etc";

        // Act & Assert
        await Assert.ThrowsAsync<PathTraversalException>(
            () => _storage.ListAsync(maliciousPath));
    }

    [Fact]
    public async Task ListAsync_WithEncodedPathTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = "..%2F..%2F..%2Fetc";

        // Act & Assert - El path se normaliza primero, así que no debería haber traversal
        // Este test verifica que los .. se eliminan durante la normalización
        var exception = await Record.ExceptionAsync(
            () => _storage.ListAsync(maliciousPath));

        // Debería lanzar PathTraversalException o StorageItemNotFoundException
        Assert.True(exception is PathTraversalException or StorageItemNotFoundException);
    }

    [Fact]
    public async Task ListAsync_WithBackslashTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = @"..\..\..\etc";

        // Act & Assert
        await Assert.ThrowsAsync<PathTraversalException>(
            () => _storage.ListAsync(maliciousPath));
    }

    [Fact]
    public async Task CreateFolderAsync_WithPathTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = "../outside_root";

        // Act & Assert
        await Assert.ThrowsAsync<PathTraversalException>(
            () => _storage.CreateFolderAsync(maliciousPath));
    }

    [Fact]
    public async Task UploadAsync_WithPathTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = "../../../";
        using var stream = new MemoryStream("test content"u8.ToArray());

        // Act & Assert
        await Assert.ThrowsAsync<PathTraversalException>(
            () => _storage.UploadAsync(maliciousPath, "test.txt", stream, "text/plain"));
    }

    [Fact]
    public async Task DeleteAsync_WithPathTraversal_ThrowsException()
    {
        // Arrange
        var maliciousPath = "../../important_file";

        // Act & Assert
        await Assert.ThrowsAsync<PathTraversalException>(
            () => _storage.DeleteAsync(maliciousPath));
    }

    [Fact]
    public async Task RenameAsync_WithPathTraversal_ThrowsException()
    {
        // Arrange - Crear archivo válido primero
        var validFile = "test.txt";
        var filePath = Path.Combine(_testRootPath, validFile);
        await File.WriteAllTextAsync(filePath, "test");

        // Act & Assert - Intentar renombrar con path traversal en nuevo nombre
        await Assert.ThrowsAsync<InvalidFileNameException>(
            () => _storage.RenameAsync(validFile, "../malicious.txt"));
    }

    #endregion

    #region Normal Operation Tests

    [Fact]
    public async Task CreateFolderAsync_ValidPath_CreatesFolder()
    {
        // Arrange
        var folderName = "test_folder";

        // Act
        await _storage.CreateFolderAsync(folderName);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testRootPath, folderName)));
    }

    [Fact]
    public async Task ListAsync_EmptyDirectory_ReturnsEmptyList()
    {
        // Act
        var result = await _storage.ListAsync(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_WithItems_ReturnsFoldersFirst()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testRootPath, "folder_b"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "folder_a"));
        await File.WriteAllTextAsync(Path.Combine(_testRootPath, "file_a.txt"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testRootPath, "file_b.txt"), "content");

        // Act
        var result = await _storage.ListAsync(null);

        // Assert
        Assert.Equal(4, result.Count);

        // Primero las carpetas, ordenadas alfabéticamente
        Assert.True(result[0].IsDirectory);
        Assert.Equal("folder_a", result[0].Name);
        Assert.True(result[1].IsDirectory);
        Assert.Equal("folder_b", result[1].Name);

        // Luego los archivos, ordenados alfabéticamente
        Assert.False(result[2].IsDirectory);
        Assert.Equal("file_a.txt", result[2].Name);
        Assert.False(result[3].IsDirectory);
        Assert.Equal("file_b.txt", result[3].Name);
    }

    [Fact]
    public async Task UploadAsync_ValidFile_UploadsSuccessfully()
    {
        // Arrange
        var fileName = "uploaded.txt";
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        await _storage.UploadAsync("", fileName, stream, "text/plain");

        // Assert
        var filePath = Path.Combine(_testRootPath, fileName);
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        var fileName = "to_delete.txt";
        var filePath = Path.Combine(_testRootPath, fileName);
        await File.WriteAllTextAsync(filePath, "content");

        // Act
        await _storage.DeleteAsync(fileName);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task RenameAsync_ExistingFile_RenamesFile()
    {
        // Arrange
        var originalName = "original.txt";
        var newName = "renamed.txt";
        var originalPath = Path.Combine(_testRootPath, originalName);
        await File.WriteAllTextAsync(originalPath, "content");

        // Act
        await _storage.RenameAsync(originalName, newName);

        // Assert
        Assert.False(File.Exists(originalPath));
        Assert.True(File.Exists(Path.Combine(_testRootPath, newName)));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UploadAsync_FileSizeExceeded_ThrowsException()
    {
        // Arrange - Crear opciones con límite pequeño
        var options = Options.Create(new StorageOptions
        {
            RootPath = _testRootPath,
            MaxFileSizeBytes = 10, // Solo 10 bytes
            AllowedExtensions = new List<string>(),
            EnableThumbnailCache = false
        });
        var storage = new LocalDiskFileStorage(options, NullLogger<LocalDiskFileStorage>.Instance);

        var content = new byte[100]; // 100 bytes, excede el límite
        using var stream = new MemoryStream(content);

        // Act & Assert
        await Assert.ThrowsAsync<FileSizeExceededException>(
            () => storage.UploadAsync("", "large.txt", stream, "text/plain"));
    }

    [Fact]
    public async Task UploadAsync_InvalidFileName_ThrowsException()
    {
        // Arrange
        var invalidFileName = ".hidden_file";
        using var stream = new MemoryStream("test"u8.ToArray());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidFileNameException>(
            () => _storage.UploadAsync("", invalidFileName, stream, "text/plain"));
    }

    [Fact]
    public async Task CreateFolderAsync_ExistingFolder_ThrowsException()
    {
        // Arrange
        var folderName = "existing_folder";
        Directory.CreateDirectory(Path.Combine(_testRootPath, folderName));

        // Act & Assert
        await Assert.ThrowsAsync<StorageItemExistsException>(
            () => _storage.CreateFolderAsync(folderName));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingItem_ThrowsException()
    {
        // Arrange
        var nonExisting = "non_existing_file.txt";

        // Act & Assert
        await Assert.ThrowsAsync<StorageItemNotFoundException>(
            () => _storage.DeleteAsync(nonExisting));
    }

    [Fact]
    public async Task DeleteAsync_NonEmptyFolderWithoutRecursive_ThrowsException()
    {
        // Arrange
        var folderName = "non_empty_folder";
        var folderPath = Path.Combine(_testRootPath, folderName);
        Directory.CreateDirectory(folderPath);
        await File.WriteAllTextAsync(Path.Combine(folderPath, "file.txt"), "content");

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotEmptyException>(
            () => _storage.DeleteAsync(folderName, recursive: false));
    }

    [Fact]
    public async Task DeleteAsync_NonEmptyFolderWithRecursive_DeletesAll()
    {
        // Arrange
        var folderName = "folder_to_delete";
        var folderPath = Path.Combine(_testRootPath, folderName);
        Directory.CreateDirectory(folderPath);
        await File.WriteAllTextAsync(Path.Combine(folderPath, "file.txt"), "content");

        // Act
        await _storage.DeleteAsync(folderName, recursive: true);

        // Assert
        Assert.False(Directory.Exists(folderPath));
    }

    #endregion
}
