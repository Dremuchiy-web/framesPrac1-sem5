namespace Task1.Framework.Domain;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

