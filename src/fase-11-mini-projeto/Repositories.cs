using System;
using System.Collections.Generic;

namespace Fase11_MiniProjeto;

/// <summary>
/// Contrato ISP: apenas operações de leitura.
/// Clientes que só consultam dependem deste.
/// </summary>
public interface IReadRepository<T, TId>
{
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
}

/// <summary>
/// Contrato ISP: apenas operações de escrita.
/// Clientes que modificam dependem deste.
/// </summary>
public interface IWriteRepository<T, TId>
{
    T Add(T entity);
    bool Update(T entity);
    bool Remove(TId id);
}

/// <summary>
/// Contrato completo: composição de leitura + escrita.
/// Útil para serviços que precisam de ambas as capacidades.
/// </summary>
public interface IRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId>
{
}

/// <summary>
/// Implementação em memória para testes (dublê).
/// Sem I/O, determinístico, rápido.
/// </summary>
public sealed class InMemoryRepository<T, TId> : IRepository<T, TId>
    where TId : notnull
{
    private readonly Func<T, TId> _idSelector;
    private readonly Dictionary<TId, T> _store;

    public int OperationCount { get; private set; }

    public InMemoryRepository(Func<T, TId> idSelector)
    {
        _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        _store = new Dictionary<TId, T>();
    }

    public T Add(T entity)
    {
        var id = _idSelector(entity);
        _store[id] = entity;
        OperationCount++;
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
            return false;

        _store[id] = entity;
        OperationCount++;
        return true;
    }

    public bool Remove(TId id)
    {
        var removed = _store.Remove(id);
        if (removed)
            OperationCount++;
        return removed;
    }
}
