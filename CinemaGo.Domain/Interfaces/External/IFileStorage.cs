namespace CinemaGo.Domain.Interfaces.External;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken ct = default);
    void Delete(string? relativePath);
}