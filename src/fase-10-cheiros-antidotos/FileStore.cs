using System.Collections.Generic;
using System.IO;

namespace Fase10_CheirosAntidotos;

public interface IFileStore
{
    bool Exists(string path);
    string Read(string path);
    void Write(string path, string content);
}

public sealed class DiskFileStore : IFileStore
{
    public bool Exists(string path) => File.Exists(path);
    public string Read(string path) => File.ReadAllText(path);
    public void Write(string path, string content) => File.WriteAllText(path, content);
}

public sealed class InMemoryFileStore : IFileStore
{
    private readonly Dictionary<string, string> _files = new();

    public int WriteCount { get; private set; }

    public bool Exists(string path) => _files.ContainsKey(path);

    public string Read(string path) => _files.TryGetValue(path, out var content) ? content : string.Empty;

    public void Write(string path, string content)
    {
        _files[path] = content;
        WriteCount++;
    }
}

public sealed class DocumentRepository
{
    private readonly IFileStore _store;

    public DocumentRepository(IFileStore store)
    {
        _store = store;
    }

    public void Save(Document doc, string path)
    {
        var payload = $"{doc.Id}|{doc.Content}";
        _store.Write(path, payload);
    }

    public Document? Load(string path)
    {
        if (!_store.Exists(path))
        {
            return null;
        }

        var raw = _store.Read(path);
        var parts = raw.Split('|');
        if (parts.Length < 2)
        {
            return null;
        }

        return new Document(parts[0], parts[1]);
    }
}
