using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase11_MiniProjeto;

/// <summary>
/// Serviço de domínio para operações de catálogo.
/// Depende apenas dos contratos ISP que necessita.
/// Aplica validações de negócio.
/// </summary>
public sealed class CatalogService
{
    private readonly IReadRepository<Book, int> _read;
    private readonly IWriteRepository<Book, int> _write;

    public CatalogService(IReadRepository<Book, int> read, IWriteRepository<Book, int> write)
    {
        _read = read ?? throw new ArgumentNullException(nameof(read));
        _write = write ?? throw new ArgumentNullException(nameof(write));
    }

    /// <summary>Adiciona novo livro ao catálogo.</summary>
    public Book Register(Book book)
    {
        ValidateBook(book);

        var existing = _read.GetById(book.Id);
        if (existing != null)
            throw new InvalidOperationException($"Livro com ID {book.Id} já existe.");

        return _write.Add(book);
    }

    /// <summary>Lista todos os livros.</summary>
    public IReadOnlyList<Book> ListAll()
    {
        return _read.ListAll();
    }

    /// <summary>Busca livro por ID.</summary>
    public Book? FindById(int id)
    {
        return _read.GetById(id);
    }

    /// <summary>Busca livros por autor (case-insensitive).</summary>
    public IReadOnlyList<Book> FindByAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new List<Book>();

        return _read.ListAll()
                    .Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
                    .ToList();
    }

    /// <summary>Busca livros por título (case-insensitive).</summary>
    public IReadOnlyList<Book> FindByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new List<Book>();

        return _read.ListAll()
                    .Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                    .ToList();
    }

    /// <summary>Atualiza título de um livro.</summary>
    public bool UpdateTitle(int id, string newTitle)
    {
        var book = _read.GetById(id);
        if (book == null)
            return false;

        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Título não pode ser vazio.", nameof(newTitle));

        return _write.Update(book with { Title = newTitle });
    }

    /// <summary>Remove livro do catálogo.</summary>
    public bool RemoveBook(int id)
    {
        return _write.Remove(id);
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
