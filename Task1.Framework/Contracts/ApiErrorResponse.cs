namespace Task1.Framework.Contracts;

public sealed record ApiErrorResponse(string Code, string Message, string RequestId);

