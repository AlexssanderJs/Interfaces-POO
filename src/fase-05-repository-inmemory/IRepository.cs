using System.Collections.Generic;

namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Contrato genérico do padrão Repository.
    /// Define operações de acesso a dados SEM regras de negócio.
    /// O cliente fala apenas com este contrato, nunca diretamente com coleções.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade (agregado)</typeparam>
    /// <typeparam name="TId">Tipo do identificador da entidade</typeparam>
    public interface IRepository<T, TId>
    {
        /// <summary>
        /// Adiciona uma nova entidade ao repositório.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns>A entidade adicionada (para facilitar fluência)</returns>
        T Add(T entity);

        /// <summary>
        /// Busca uma entidade pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da entidade</param>
        /// <returns>A entidade encontrada ou null se não existir</returns>
        T? GetById(TId id);

        /// <summary>
        /// Lista todas as entidades do repositório.
        /// Retorna IReadOnlyList para não expor coleções mutáveis.
        /// </summary>
        /// <returns>Lista somente leitura de todas as entidades</returns>
        IReadOnlyList<T> ListAll();

        /// <summary>
        /// Atualiza uma entidade existente.
        /// </summary>
        /// <param name="entity">Entidade com dados atualizados</param>
        /// <returns>true se atualizou; false se não existe</returns>
        bool Update(T entity);

        /// <summary>
        /// Remove uma entidade pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da entidade a ser removida</param>
        /// <returns>true se removeu; false se não existe</returns>
        bool Remove(TId id);
    }
}
