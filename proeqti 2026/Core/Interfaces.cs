namespace proeqti_2026;

public interface IEntity
{
    string Id { get; }
}

public interface IRepository<T> where T : IEntity
{
    List<T> Items { get; }
    T? Find(string id);
    void Add(T item);
}

public sealed class Repository<T> : IRepository<T> where T : IEntity
{
    public List<T> Items { get; } = new();

    public T? Find(string id) => Items.FirstOrDefault(item =>
        item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public void Add(T item) => Items.Add(item);
}
