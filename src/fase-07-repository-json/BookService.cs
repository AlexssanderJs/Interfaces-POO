using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Serviço de domínio para livros.
    /// Encapsula lógica de negócio (validação) separada do Repository (acesso a dados).
    /// </summary>
    public static class BookService
    {
        /// <summary>Registra novo livro com validação.</summary>
        public static Book Register(IRepository<Book, int> repo, Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Título obrigatório");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Autor obrigatório");

            if (book.Year < 1000 || book.Year > DateTime.Now.Year)
                throw new ArgumentException($"Ano deve estar entre 1000 e {DateTime.Now.Year}");

            return repo.Add(book);
        }

        /// <summary>Busca livro por ID.</summary>
        public static Book? FindById(IRepository<Book, int> repo, int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido");

            return repo.GetById(id);
        }

        /// <summary>Lista todos os livros cadastrados.</summary>
        public static IReadOnlyList<Book> ListAll(IRepository<Book, int> repo)
        {
            return repo.ListAll();
        }

        /// <summary>Atualiza livro existente com validação.</summary>
        public static bool Update(IRepository<Book, int> repo, Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Título obrigatório");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Autor obrigatório");

            return repo.Update(book);
        }

        /// <summary>Remove livro por ID.</summary>
        public static bool Remove(IRepository<Book, int> repo, int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido");

            return repo.Remove(id);
        }
    }
}
