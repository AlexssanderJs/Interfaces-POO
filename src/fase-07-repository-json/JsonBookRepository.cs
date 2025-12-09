using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Implementação do Repository usando JSON (System.Text.Json).
    /// Persiste livros em arquivo JSON com serialização camelCase e indentado.
    /// </summary>
    public sealed class JsonBookRepository : IRepository<Book, int>
    {
        private readonly string _path;

        /// <summary>Opções de serialização: camelCase, ignora nulls, indentado para legibilidade.</summary>
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        /// <summary>Inicializa o repositório com caminho do arquivo JSON.</summary>
        public JsonBookRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path inválido", nameof(path));
            _path = path;
        }

        /// <summary>Insere ou substitui (upsert) livro no repositório.</summary>
        public Book Add(Book entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var list = Load();
            list.RemoveAll(b => b.Id == entity.Id); // remove se existe
            list.Add(entity);
            Save(list);
            return entity;
        }

        /// <summary>Busca livro por ID. Retorna null se não encontrado.</summary>
        public Book? GetById(int id)
        {
            return Load().FirstOrDefault(b => b.Id == id);
        }

        /// <summary>Lista todos os livros cadastrados.</summary>
        public IReadOnlyList<Book> ListAll()
        {
            return Load();
        }

        /// <summary>Atualiza livro existente. Retorna false se não encontrado.</summary>
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

        /// <summary>Remove livro por ID. Retorna false se não encontrado.</summary>
        public bool Remove(int id)
        {
            var list = Load();
            var removed = list.RemoveAll(b => b.Id == id) > 0;

            if (removed)
            {
                Save(list);
            }

            return removed;
        }

        // ========== Helpers Privados ==========

        /// <summary>
        /// Carrega livros do arquivo JSON.
        /// Se arquivo não existe ou está vazio, retorna lista vazia.
        /// </summary>
        private List<Book> Load()
        {
            if (!File.Exists(_path))
                return new List<Book>();

            var json = File.ReadAllText(_path, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
                return new List<Book>();

            try
            {
                return JsonSerializer.Deserialize<List<Book>>(json, _opts) ?? new List<Book>();
            }
            catch (JsonException)
            {
                // JSON inválido = lista vazia (falha gracefully)
                return new List<Book>();
            }
        }

        /// <summary>
        /// Salva livros em arquivo JSON com formatação legível.
        /// </summary>
        private void Save(List<Book> list)
        {
            var json = JsonSerializer.Serialize(list, _opts);
            File.WriteAllText(_path, json, Encoding.UTF8);
        }
    }
}
