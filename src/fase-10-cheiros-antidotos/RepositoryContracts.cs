using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase10_CheirosAntidotos;

public interface IReadRepository<T, TId>
{
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
}

public interface IWriteRepository<T, TId>
{
    T Add(T entity);
    bool Update(T entity);
    bool Remove(TId id);
}

public interface IRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId>
{
}

public static class ReadOnlyReportService
{
    public static IReadOnlyList<string> ListTitles(IReadRepository<Book, int> repo)
    {
        return repo.ListAll().Select(b => b.Title).ToList();
    }

    public static string? FindTitle(IReadRepository<Book, int> repo, int id)
    {
        return repo.GetById(id)?.Title;
    }
}

public sealed class ReadOnlyRepoFake : IReadRepository<Book, int>
{
    private readonly List<Book> _items;

    public ReadOnlyRepoFake(IEnumerable<Book> seed)
    {
        _items = seed.ToList();
    }

    public Book? GetById(int id) => _items.FirstOrDefault(b => b.Id == id);

    public IReadOnlyList<Book> ListAll() => _items;
}

public sealed class InMemoryRepository<T, TId> : IRepository<T, TId>
    where TId : notnull
{
    private readonly Func<T, TId> _idSelector;
    private readonly Dictionary<TId, T> _store;

    public InMemoryRepository(Func<T, TId> idSelector)
    {
        _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        _store = new Dictionary<TId, T>();
    }

    public T Add(T entity)
    {
        var id = _idSelector(entity);
        _store[id] = entity;
        return entity;
    }

    public T? GetById(TId id)
    {
        return _store.TryGetValue(id, out var entity) ? entity : default;
    }

    public IReadOnlyList<T> ListAll()
    {
        return _store.Values.ToList();
    }

    public bool Update(T entity)
    {
        var id = _idSelector(entity);
        if (!_store.ContainsKey(id))
        {
            return false;
        }

        _store[id] = entity;
        return true;
    }

    public bool Remove(TId id)
    {
        return _store.Remove(id);
    }
}
