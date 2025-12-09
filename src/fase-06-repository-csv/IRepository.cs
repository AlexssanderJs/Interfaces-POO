using System.Collections.Generic;

namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Contrato genérico do padrão Repository.
    /// Reaproveitado da Fase 5 - funciona para qualquer tipo de persistência.
    /// </summary>
    public interface IRepository<T, TId>
    {
        T Add(T entity);
        T? GetById(TId id);
        IReadOnlyList<T> ListAll();
        bool Update(T entity);
        bool Remove(TId id);
    }
}
