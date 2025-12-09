using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Implementação do Repository com persistência em arquivo CSV.
    /// Mantém o mesmo contrato IRepository&lt;T, TId&gt; da Fase 5.
    /// 
    /// Características:
    /// - Arquivo CSV com cabeçalho (Id,Title,Author,Year)
    /// - Escape correto de aspas e vírgulas
    /// - Encoding UTF-8
    /// - Leitura/escrita completa (sem concorrência)
    /// </summary>
    public sealed class CsvBookRepository : IRepository<Book, int>
    {
        private readonly string _path;

        /// <summary>
        /// Cria um repositório CSV.
        /// </summary>
        /// <param name="path">Caminho do arquivo CSV</param>
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

            // Política: se já existe ID, substitui; caso contrário, adiciona
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
            {
                Save(list);
            }

            return removed;
        }

        // ========== Helpers Privados ==========

        /// <summary>
        /// Carrega todos os livros do arquivo CSV.
        /// Se o arquivo não existe, retorna lista vazia.
        /// Respeita quebras de linha dentro de campos quoted.
        /// </summary>
        private List<Book> Load()
        {
            if (!File.Exists(_path))
                return new List<Book>();

            var content = File.ReadAllText(_path, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(content))
                return new List<Book>();

            var list = new List<Book>();
            var lines = ParseCsvLines(content);

            if (lines.Count == 0)
                return list;

            var startIndex = 0;

            // Detecta e pula cabeçalho
            if (lines[0].StartsWith("Id,"))
                startIndex = 1;

            for (int i = startIndex; i < lines.Count; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var cols = SplitCsvLine(line);

                if (cols.Count < 4)
                    continue; // ignora linha quebrada

                if (!int.TryParse(cols[0], out var id))
                    continue;

                var title = cols[1];
                var author = cols[2];

                if (!int.TryParse(cols[3], out var year))
                    year = 0;

                list.Add(new Book(id, title, author, year));
            }

            return list;
        }

        /// <summary>
        /// Salva todos os livros no arquivo CSV com cabeçalho.
        /// </summary>
        private void Save(List<Book> books)
        {
            var sb = new StringBuilder();

            // Escreve cabeçalho
            sb.AppendLine("Id,Title,Author,Year");

            // Escreve dados ordenados por ID
            foreach (var book in books.OrderBy(b => b.Id))
            {
                var id = book.Id.ToString();
                var title = Escape(book.Title);
                var author = Escape(book.Author);
                var year = book.Year.ToString();

                sb.Append(id).Append(',')
                  .Append(title).Append(',')
                  .Append(author).Append(',')
                  .Append(year).AppendLine();
            }

            File.WriteAllText(_path, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Escapa valores CSV: aspas escapadas e quotes quando necessário.
        /// Regras:
        /// - Aspas duplas escapam como ""
        /// - Se contém , " \n \r, envolve com aspas
        /// </summary>
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var needsQuotes = value.Contains(',') ||
                              value.Contains('"') ||
                              value.Contains('\n') ||
                              value.Contains('\r');

            var escaped = value.Replace("\"", "\"\"");

            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }

        /// <summary>
        /// Faz parsing de uma linha CSV respeitando quotes e escapes.
        /// </summary>
        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(line))
            {
                result.Add(string.Empty);
                return result;
            }

            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Possível aspas escapada
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // consome a segunda aspas
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result;
        }

        /// <summary>
        /// Parse de linhas CSV respeitando campos quoted (que podem conter quebras de linha).
        /// </summary>
        private static List<string> ParseCsvLines(string content)
        {
            var lines = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                var c = content[i];

                if (inQuotes)
                {
                    if (c == '"' && i + 1 < content.Length && content[i + 1] == '"')
                    {
                        // Aspa escapada (duplicada)
                        current.Append("\"\"");
                        i++;
                    }
                    else if (c == '"')
                    {
                        // Fim do campo quoted
                        current.Append('"');
                        inQuotes = false;
                    }
                    else
                    {
                        // Caractere normal, incluindo quebras de linha
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '\n')
                    {
                        // Fim da linha CSV (não está dentro de aspas)
                        if (current.Length > 0)
                        {
                            lines.Add(current.ToString());
                            current.Clear();
                        }
                    }
                    else if (c == '"')
                    {
                        // Início de campo quoted - adiciona a aspa
                        current.Append('"');
                        inQuotes = true;
                    }
                    else if (c != '\r') // ignora \r (carriage return)
                    {
                        current.Append(c);
                    }
                }
            }

            // Adiciona última linha se houver
            if (current.Length > 0)
            {
                lines.Add(current.ToString());
            }

            return lines;
        }
    }
}
