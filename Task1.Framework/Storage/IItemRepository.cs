using Task1.Framework.Domain;

namespace Task1.Framework.Storage;

public interface IItemRepository
{
    IEnumerable<Item> GetAll();
    Item? GetById(int id);
    Item Create(ItemCreate create);
}

