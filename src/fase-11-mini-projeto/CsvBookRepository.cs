using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fase11_MiniProjeto;

/// <summary>
/// Repositório com persistência em CSV.
/// Segue mesmo padrão da Fase 6, adaptado para Book.
/// </summary>
public sealed class CsvBookRepository : IRepository<Book, int>
{
    private readonly string _path;

    public CsvBookRepository(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path inválido", nameof(path));
        _path = path;
    }

    public Book Add(Book entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var list = Load();
        list.RemoveAll(b => b.Id == entity.Id);
        list.Add(entity);
        Save(list);
        return entity;
    }

    public Book? GetById(int id)
    {
        return Load().FirstOrDefault(b => b.Id == id);
    }

    public IReadOnlyList<Book> ListAll()
    {
        return Load();
    }

    public bool Update(Book entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var list = Load();
        var index = list.FindIndex(b => b.Id == entity.Id);
        if (index < 0)
            return false;

        list[index] = entity;
        Save(list);
        return true;
    }

    public bool Remove(int id)
    {
        var list = Load();
        var removed = list.RemoveAll(b => b.Id == id) > 0;
        if (removed)
            Save(list);
        return removed;
    }

    private List<Book> Load()
    {
        if (!File.Exists(_path))
            return new List<Book>();

        var content = File.ReadAllText(_path, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(content))
            return new List<Book>();

        var list = new List<Book>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        bool isHeader = true;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (isHeader && line.StartsWith("Id,"))
            {
                isHeader = false;
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 4)
                continue;

            if (!int.TryParse(parts[0], out var id))
                continue;

            var title = UnEscape(parts[1]);
            var author = UnEscape(parts[2]);

            if (!int.TryParse(parts[3], out var year))
                year = 0;

            list.Add(new Book(id, title, author, year));
        }

        return list;
    }

    private void Save(List<Book> books)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Title,Author,Year");

        foreach (var book in books.OrderBy(b => b.Id))
        {
            sb.Append(book.Id).Append(',')
              .Append(Escape(book.Title)).Append(',')
              .Append(Escape(book.Author)).Append(',')
              .Append(book.Year).AppendLine();
        }

        File.WriteAllText(_path, sb.ToString(), Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }

    private static string UnEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
        {
            trimmed = trimmed.Substring(1, trimmed.Length - 2);
        }

        return trimmed.Replace("\"\"", "\"");
    }
}
