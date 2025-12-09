using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Implementação InMemory do padrão Repository.
    /// Usa Dictionary para armazenamento em memória (sem I/O, sem banco de dados).
    /// Ideal para testes, prototipagem e desenvolvimento.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <typeparam name="TId">Tipo do identificador (deve ser não-nulo)</typeparam>
    public sealed class InMemoryRepository<T, TId> : IRepository<T, TId>
        where TId : notnull
    {
        private readonly Dictionary<TId, T> _store = new();
        private readonly Func<T, TId> _getId;

        /// <summary>
        /// Construtor que recebe a função para extrair o ID da entidade.
        /// Política de ID: o ID vem da entidade (não é gerado aqui).
        /// </summary>
        /// <param name="getId">Função que extrai o ID de uma entidade</param>
        public InMemoryRepository(Func<T, TId> getId)
        {
            _getId = getId ?? throw new ArgumentNullException(nameof(getId));
        }

        public T Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = _getId(entity);
            _store[id] = entity; // Adiciona ou substitui (upsert)
            return entity;
        }

        public T? GetById(TId id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return _store.TryGetValue(id, out var entity) ? entity : default;
        }

        public IReadOnlyList<T> ListAll()
        {
            // Retorna lista somente leitura - cliente não pode modificar a coleção interna
            return _store.Values.ToList();
        }

        public bool Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = _getId(entity);

            // Só atualiza se existir
            if (!_store.ContainsKey(id))
                return false;

            _store[id] = entity;
            return true;
        }

        public bool Remove(TId id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            // Dictionary.Remove já retorna true/false
            return _store.Remove(id);
        }

        /// <summary>
        /// Método auxiliar para verificar se uma entidade existe.
        /// </summary>
        public bool Exists(TId id)
        {
            return _store.ContainsKey(id);
        }

        /// <summary>
        /// Método auxiliar para contar entidades.
        /// </summary>
        public int Count()
        {
            return _store.Count;
        }

        /// <summary>
        /// Método auxiliar para limpar o repositório (útil em testes).
        /// </summary>
        public void Clear()
        {
            _store.Clear();
        }
    }
}
