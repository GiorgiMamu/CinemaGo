using CinemaGo.Domain.Interfaces.External;
namespace CinemaGo.Infrastructure.FileStorage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _webRootPath;
    public LocalFileStorage(string webRootPath) => _webRootPath = webRootPath;

    public async Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken ct = default)
    {
        var directory = Path.Combine(_webRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);

        // random name: the client's file name is never used (prevents overwrites and path tricks)
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(directory, fileName);

        if (content.CanSeek) content.Position = 0;
        await using var target = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, ct);

        return $"/uploads/{folder}/{fileName}";
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var relative = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_webRootPath, relative));
        var uploadsRoot = Path.GetFullPath(Path.Combine(_webRootPath, "uploads")) + Path.DirectorySeparatorChar;

        // path-traversal guard: only files inside wwwroot/uploads may be deleted
        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}