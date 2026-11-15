using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using FluentValidation;
namespace CinemaGo.Application.Validators;

public class FileUploadValidator : AbstractValidator<FileUpload>
{
    public const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public FileUploadValidator()
    {
        RuleFor(f => f.Length).GreaterThan(0).WithMessage(ErrorMessages.EmptyFile).OverridePropertyName("file")
            .LessThanOrEqualTo(MaxBytes).WithMessage(ErrorMessages.FileTooLarge).OverridePropertyName("file");

        RuleFor(f => f.FileName)
            .Must(n => AllowedExtensions.Contains(Path.GetExtension(n).ToLowerInvariant()))
            .WithMessage(ErrorMessages.InvalidFileType).OverridePropertyName("file");

        // the extension can be faked, so the first bytes (file signature) must match a real image too
        RuleFor(f => f.Content).Must(HasImageSignature)
            .WithMessage(ErrorMessages.InvalidFileType).OverridePropertyName("file")
            .When(f => f.Length > 0);
    }

    private static bool HasImageSignature(Stream s)
    {
        if (!s.CanRead || !s.CanSeek) return false;
        var h = new byte[12];
        var read = s.Read(h, 0, h.Length);
        s.Position = 0;
        if (read < 12) return false;

        var jpeg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        var png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 && h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A;
        var webp = h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50;
        return jpeg || png || webp;
    }
}