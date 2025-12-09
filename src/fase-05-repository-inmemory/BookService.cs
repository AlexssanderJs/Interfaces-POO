using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Serviço de domínio para operações de negócio relacionadas a livros.
    /// Fala APENAS com o Repository, nunca com coleções diretamente.
    /// Contém validações e regras de negócio.
    /// </summary>
    public static class BookService
    {
        /// <summary>
        /// Registra um novo livro no repositório.
        /// Aplica validações de negócio antes de persistir.
        /// </summary>
        public static Book Register(IRepository<Book, int> repo, Book book)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            // Validações de negócio
            ValidateBook(book);

            // Verifica se já existe
            var existing = repo.GetById(book.Id);
            if (existing != null)
                throw new InvalidOperationException($"Livro com ID {book.Id} já existe.");

            return repo.Add(book);
        }

        /// <summary>
        /// Lista todos os livros cadastrados.
        /// </summary>
        public static IReadOnlyList<Book> ListAll(IRepository<Book, int> repo)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.ListAll();
        }

        /// <summary>
        /// Busca um livro pelo ID.
        /// </summary>
        public static Book? FindById(IRepository<Book, int> repo, int id)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.GetById(id);
        }

        /// <summary>
        /// Atualiza um livro existente.
        /// </summary>
        public static bool Update(IRepository<Book, int> repo, Book book)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            ValidateBook(book);

            return repo.Update(book);
        }

        /// <summary>
        /// Remove um livro pelo ID.
        /// </summary>
        public static bool Remove(IRepository<Book, int> repo, int id)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.Remove(id);
        }

        /// <summary>
        /// Busca livros por autor.
        /// Demonstra consulta personalizada usando o repositório.
        /// </summary>
        public static IReadOnlyList<Book> FindByAuthor(IRepository<Book, int> repo, string author)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            if (string.IsNullOrWhiteSpace(author))
                return new List<Book>();

            return repo.ListAll()
                       .Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
                       .ToList();
        }

        /// <summary>
        /// Busca livros por ano.
        /// </summary>
        public static IReadOnlyList<Book> FindByYear(IRepository<Book, int> repo, int year)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.ListAll()
                       .Where(b => b.Year == year)
                       .ToList();
        }

        /// <summary>
        /// Validações de negócio para livros.
        /// </summary>
        private static void ValidateBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (book.Id <= 0)
                throw new ArgumentException("ID deve ser maior que zero.", nameof(book));

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Título é obrigatório.", nameof(book));

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Autor é obrigatório.", nameof(book));

            if (book.Year < 1000 || book.Year > DateTime.Now.Year + 1)
                throw new ArgumentException("Ano inválido.", nameof(book));
        }
    }
}
