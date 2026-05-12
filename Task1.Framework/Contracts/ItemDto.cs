using Task1.Framework.Domain;

namespace Task1.Framework.Contracts;

public sealed record ItemDto(int Id, string Title, int Points)
{
    public static ItemDto FromDomain(Item item) => new(item.Id, item.Title, item.Points);
}

