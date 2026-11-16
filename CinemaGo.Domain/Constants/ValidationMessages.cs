namespace CinemaGo.Domain.Constants;

// Every sentence about INVALID INPUT. Attribute arguments must be compile-time constants, hence const strings.
public static class ValidationMessages
{
    // ---- DataAnnotations templates: {0} = field name, {1}/{2} = limits ----
    public const string Required = "{0} is required.";
    public const string Email = "{0} is not a valid email address.";
    public const string MaxLength = "{0} cannot exceed {1} characters.";                    // StringLength(max)
    public const string LengthBetween = "{0} must be between {2} and {1} characters long."; // StringLength(max, MinimumLength = min)
    public const string RangeBetween = "{0} must be between {1} and {2}.";                  // Range(min, max)
    public const string Compare = "{0} does not match {1}.";                                // Compare(other)
    public const string MinItems = "{0} must contain at least {1} item(s).";                // MinLength on lists
    public const string MaxItems = "{0} can contain at most {1} item(s).";                  // MaxLength on lists
    public const string SixDigitCode = "The code must be exactly 6 digits.";

    // ---- rule messages used by the FluentValidation validators ----
    public const string UserNamePattern = "User name may contain only letters, digits, '.', '_' and '-'.";
    public const string PasswordUppercase = "Password must contain an uppercase letter.";
    public const string PasswordLowercase = "Password must contain a lowercase letter.";
    public const string PasswordDigit = "Password must contain a digit.";
    public const string PasswordSameAsCurrent = "The new password must differ from the current one.";
    public const string InvalidRole = "Only the Admin and Customer roles can be assigned.";
    public const string GenreIdsUnique = "Genre ids must be unique.";
    public const string GenreIdPositive = "Genre id must be positive.";
    public const string SeatIdsUnique = "Seat ids must be unique.";
    public const string SeatIdPositive = "Seat id must be positive.";
    public const string StartMustBeFuture = "Start time must be in the future (send UTC).";
    public const string VipBelowStandard = "VIP price cannot be lower than the standard price.";
    public const string FileEmpty = "The uploaded file is empty.";
    public const string FileTooLarge = "The file exceeds the maximum allowed size.";
    public const string FileInvalidType = "Only JPG, PNG or WEBP images are allowed.";
}