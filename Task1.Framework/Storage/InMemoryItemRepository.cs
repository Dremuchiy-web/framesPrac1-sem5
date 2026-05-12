using System.Collections.Concurrent;
using System.Threading;
using Task1.Framework.Domain;

namespace Task1.Framework.Storage;

public sealed class InMemoryItemRepository : IItemRepository
{
    private readonly ConcurrentDictionary<int, Item> _items = new();
    private int _lastId;

    public InMemoryItemRepository()
    {
        Create(new ItemCreate("Лабораторная 1: middleware", 3));
        Create(new ItemCreate("Сделать отчёт по эксперименту", 2));
    }

    public IEnumerable<Item> GetAll() => _items.Values;

    public Item? GetById(int id) => _items.TryGetValue(id, out var item) ? item : null;

    public Item Create(ItemCreate create)
    {
        var id = Interlocked.Increment(ref _lastId);
        var item = new Item(id, create.Title, create.Points);

        if (!_items.TryAdd(id, item))
            throw new InvalidOperationException($"Failed to add item with id={id}.");

        return item;
    }
}

