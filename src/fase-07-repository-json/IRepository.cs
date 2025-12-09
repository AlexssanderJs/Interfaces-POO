using System.Collections.Generic;

namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Contrato genérico para persistência de dados.
    /// Desacopla o cliente da implementação específica (InMemory, CSV, JSON, SQL, etc).
    /// </summary>
    public interface IRepository<T, TId>
    {
        /// <summary>Insere ou substitui (upsert) se o ID já existe.</summary>
        T Add(T entity);

        /// <summary>Busca por ID. Retorna null se não encontrado.</summary>
        T? GetById(TId id);

        /// <summary>Lista todos os registros.</summary>
        IReadOnlyList<T> ListAll();

        /// <summary>Atualiza se existe, retorna false caso contrário.</summary>
        bool Update(T entity);

        /// <summary>Remove se existe, retorna false caso contrário.</summary>
        bool Remove(TId id);
    }
}
