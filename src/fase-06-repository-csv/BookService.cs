using System;
using System.Collections.Generic;

namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Serviço de domínio para operações com livros.
    /// Fala apenas com a interface IRepository, não conhece a implementação CSV.
    /// </summary>
    public static class BookService
    {
        public static Book Register(IRepository<Book, int> repo, Book book)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            ValidateBook(book);

            var existing = repo.GetById(book.Id);
            if (existing != null)
                throw new InvalidOperationException($"Livro com ID {book.Id} já existe.");

            return repo.Add(book);
        }

        public static IReadOnlyList<Book> ListAll(IRepository<Book, int> repo)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.ListAll();
        }

        public static Book? FindById(IRepository<Book, int> repo, int id)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.GetById(id);
        }

        public static bool Update(IRepository<Book, int> repo, Book book)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            ValidateBook(book);
            return repo.Update(book);
        }

        public static bool Remove(IRepository<Book, int> repo, int id)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            return repo.Remove(id);
        }

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
