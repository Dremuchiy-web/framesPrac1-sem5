using Task1.Framework.Contracts;

namespace Task1.Framework.Domain;

public static class ItemValidation
{
    public static void ValidateCreate(CreateItemRequest request)
    {
        var title = request.Title?.Trim() ?? string.Empty;

        if (title.Length == 0)
            throw new DomainValidationException(ErrorCodes.ValidationError, "Title must be non-empty.");

        if (title.Length > 80)
            throw new DomainValidationException(ErrorCodes.ValidationError, "Title is too long (max 80 characters).");

        if (request.Points < 0)
            throw new DomainValidationException(ErrorCodes.ValidationError, "Points must be non-negative.");

        if (request.Points > 10_000)
            throw new DomainValidationException(ErrorCodes.ValidationError, "Points is too large (max 10000).");
    }
}

