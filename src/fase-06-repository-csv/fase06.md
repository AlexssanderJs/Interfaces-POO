## Fase 6 — Repository CSV (Persistência em Arquivo)

### Objetivo
Evoluir o Repository da Fase 5 para persistir dados em arquivo **CSV**, mantendo o mesmo contrato `IRepository<T, TId>`. Implementar escape correto de caracteres especiais (vírgulas, aspas, quebras de linha) e testes de integração com arquivo real.

---

## Arquitetura

```
┌──────────────────────────────────────────────────────┐
│                      Cliente                          │
│              (BookService / Program)                  │
└────────────────────┬─────────────────────────────────┘
                     │ depende de
                     ▼
┌──────────────────────────────────────────────────────┐
│            IRepository<Book, int>                     │
│                  (CONTRATO)                           │
└────────────────────┬─────────────────────────────────┘
                     │ implementa
                     ▼
┌──────────────────────────────────────────────────────┐
│         CsvBookRepository                            │
│         (PERSISTÊNCIA CSV)                           │
│  - Load() → Lê arquivo CSV                           │
│  - Save() → Escreve arquivo CSV                      │
│  - Escape() → Trata vírgulas e aspas                 │
│  - SplitCsvLine() → Parse de linha CSV               │
└────────────────────┬─────────────────────────────────┘
                     │ usa
                     ▼
┌──────────────────────────────────────────────────────┐
│              books.csv (arquivo)                      │
│        Id,Title,Author,Year                          │
│        1,"Livro, com vírgula","Autor Normal",2020    │
│        2,"Livro ""com"" aspas",Outro,2021            │
└──────────────────────────────────────────────────────┘
```

---

## 1. Contrato (Reaproveitado da Fase 5)

```csharp
public interface IRepository<T, TId>
{
    T Add(T entity);
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
    bool Update(T entity);
    bool Remove(TId id);
}

public sealed record Book(int Id, string Title, string Author, int Year);
```

**Vantagem:** Mesma interface funciona para InMemory (Fase 5) e CSV (Fase 6)!

---

## 2. Implementação CSV

```csharp
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
        var list = Load();
        list.RemoveAll(b => b.Id == entity.Id); // se existe, substitui
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
        var list = Load();
        var index = list.FindIndex(b => b.Id == entity.Id);
        if (index < 0) return false;
        list[index] = entity;
        Save(list);
        return true;
    }

    public bool Remove(int id)
    {
        var list = Load();
        var removed = list.RemoveAll(b => b.Id == id) > 0;
        if (removed) Save(list);
        return removed;
    }

    // ========== Helpers ==========

    private List<Book> Load()
    {
        if (!File.Exists(_path))
            return new List<Book>();

        var lines = File.ReadAllLines(_path, Encoding.UTF8);
        var list = new List<Book>();
        var startIndex = lines[0].StartsWith("Id,") ? 1 : 0; // pula cabeçalho

        for (int i = startIndex; i < lines.Length; i++)
        {
            var cols = SplitCsvLine(lines[i]);
            if (cols.Count < 4 || !int.TryParse(cols[0], out var id))
                continue;

            list.Add(new Book(id, cols[1], cols[2], 
                int.TryParse(cols[3], out var year) ? year : 0));
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
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var needsQuotes = value.Contains(',') || value.Contains('"') ||
                          value.Contains('\n') || value.Contains('\r');

        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
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
}
```

---

## 3. Formato CSV

**Cabeçalho:**
```
Id,Title,Author,Year
```

**Exemplos de dados:**
```
1,Código Limpo,Robert C. Martin,2008
2,"Livro, com vírgula",Autor Normal,2020
3,"Livro ""com"" aspas",Outro Autor,2021
4,"Título
com quebra",Autor,2022
```

**Regras de Escape:**
- Vírgulas: envolver com aspas `"Livro, título"`
- Aspas: duplicar `"" → ""`
- Quebras de linha: envolver com aspas
- Encoding: UTF-8

---

## 4. Uso pelo Cliente

```csharp
public static void Main()
{
    // Composição
    var csvPath = Path.Combine(AppContext.BaseDirectory, "books.csv");
    IRepository<Book, int> repo = new CsvBookRepository(csvPath);

    // Uso (mesmo que Fase 5!)
    BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
    BookService.Register(repo, new Book(2, "DDD", "Eric Evans", 2003));

    var all = BookService.ListAll(repo);
    foreach (var book in all)
    {
        Console.WriteLine($"#{book.Id} - {book.Title} ({book.Author})");
    }
}
```

**Observação importante:** O cliente NÃO muda! Mesma interface = polimorfismo completo!

---

## 5. Testes de Integração

### Cobertura

✅ **Operações Básicas:**
- Arquivo não existe → lista vazia
- Arquivo vazio → lista vazia
- Add + ListAll → persiste em arquivo
- GetById existente → retorna
- GetById ausente → null
- Update existente → persiste
- Update ausente → false
- Remove existente → deleta arquivo
- Remove ausente → false

✅ **Caracteres Especiais:**
- Vírgula em título → escapa com aspas
- Aspas em autor → duplica escapes
- Quebra de linha → escapa com aspas
- Múltiplos livros com tudo junto

### Exemplo de Teste

```csharp
[Fact]
public void Add_WithCommaInTitle_ShouldEscapeCorrectly()
{
    var path = CreateTempPath();
    var repo = new CsvBookRepository(path);

    repo.Add(new Book(1, "Livro, com vírgula", "Autor", 2020));
    var found = repo.GetById(1);

    Assert.Equal("Livro, com vírgula", found!.Title);
    Assert.Contains(",", found.Title);
}
```

---

## 6. Comparação: InMemory vs. CSV

| Aspecto | Fase 5 (InMemory) | Fase 6 (CSV) |
|---------|-----------------|------------|
| **Contrato** | `IRepository<T, TId>` | `IRepository<T, TId>` |
| **Persistência** | RAM (lista) | Arquivo CSV em disco |
| **Durabilidade** | Perdido ao desligar | Permanente |
| **Performance** | Muito rápido | Mais lento (I/O) |
| **Escalabilidade** | Limitado a RAM | Limitado a disco |
| **Uso** | Testes, prototipagem | Dados simples, local |

**Ganho:** Mesmo código cliente, comportamentos diferentes!

---

## 7. Como Executar

```powershell
cd c:\Projects\Interfaces\src\fase-06-repository-csv
dotnet build
dotnet run
```

### Saída Esperada

```
=== Fase 6: Repository CSV ===

--- Operações Básicas (Persistência em CSV) ---
✓ 3 livros cadastrados em CSV

Livros cadastrados:
  #1 - Código Limpo (Robert C. Martin, 2008)
  #2 - Domain-Driven Design (Eric Evans, 2003)
  #3 - Refactoring (Martin Fowler, 1999)

✓ Livro ID 2 atualizado
✓ Livro ID 3 removido. Total: 2 livros

--- Campos com Caracteres Especiais ---
Livros com caracteres especiais:
  #1 - C# com Padrões: Herança, Interfaces, etc (Martin, Robert C.)
  #2 - "Clean Code" - A Handbook (Uncle "Bob" Martin)
  #3 - Linha
com quebra
de linha (Autor Normal)

✓ Recuperado do CSV: C# com Padrões: Herança, Interfaces, etc
✓ Contém vírgula: True

--- Persistência em Arquivo ---
✓ Arquivo CSV criado: (temp).csv
✓ Tamanho: XXX bytes

Conteúdo do CSV:
  Id,Title,Author,Year
  1,"C# com Padrões: Herança, Interfaces, etc","Martin, Robert C.",2020
  2,"""Clean Code"" - A Handbook",Uncle """Bob""" Martin,2008
  ...

=== Executando Testes de Integração (CSV) ===

✓ Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty
✓ Test_ListAll_WhenFileEmpty_ShouldReturnEmpty
✓ Test_Add_Then_ListAll_ShouldPersistInFile
✓ Test_GetById_Existing_ShouldReturnBook
✓ Test_GetById_Missing_ShouldReturnNull
✓ Test_Update_Existing_ShouldPersistChanges
✓ Test_Update_Missing_ShouldReturnFalse
✓ Test_Remove_Existing_ShouldDeleteFromFile
✓ Test_Remove_Missing_ShouldReturnFalse
✓ Test_Add_WithCommaInTitle_ShouldEscapeCorrectly
✓ Test_Add_WithQuotesInAuthor_ShouldEscapeCorrectly
✓ Test_Add_WithNewlineInField_ShouldEscapeCorrectly
✓ Test_Add_MultipleBooks_ShouldPersistAllAndPersist

✅ Todos os testes de integração passaram!
```

---

## 8. Limitações (Por Design)

- ❌ **Sem concorrência:** múltiplas instâncias não sincronizam
- ❌ **Load completo:** toda leitura carrega arquivo inteiro
- ❌ **Sem índices:** GetById faz busca linear
- ❌ **Sem versionamento:** mudanças de schema quebram compatibilidade

**Próximos passos:** Fase 7 com banco de dados real (SQL)

---

## 9. Referências

- [CSV RFC 4180](https://tools.ietf.org/html/rfc4180)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [File I/O em C#](https://docs.microsoft.com/en-us/dotnet/api/system.io)
