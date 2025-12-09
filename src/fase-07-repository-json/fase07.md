## Fase 7 — Repository JSON (System.Text.Json)

### Objetivo
Evoluir o Repository da Fase 6 (CSV) para persistir dados em arquivo **JSON**, mantendo o mesmo contrato `IRepository<T, TId>`. Implementar serialização com `System.Text.Json` usando camelCase e ignorar nulls, com testes de integração completos.

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
│         JsonBookRepository                           │
│         (PERSISTÊNCIA JSON)                          │
│  - Load() → Desserializa JSON                        │
│  - Save() → Serializa JSON                           │
│  - JsonSerializerOptions → camelCase, ignora nulls   │
└────────────────────┬─────────────────────────────────┘
                     │ usa
                     ▼
┌──────────────────────────────────────────────────────┐
│              books.json (arquivo)                     │
│  [                                                   │
│    {                                                 │
│      "id": 1,                                        │
│      "title": "Código Limpo",                        │
│      "author": "Robert C. Martin",                   │
│      "year": 2008                                    │
│    },                                                │
│    ...                                               │
│  ]                                                   │
└──────────────────────────────────────────────────────┘
```

---

## 1. Contrato (Reaproveitado das Fases Anteriores)

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

**Vantagem:** Mesma interface funciona para InMemory (Fase 5), CSV (Fase 6) e JSON (Fase 7)!

---

## 2. Implementação JSON com System.Text.Json

```csharp
public sealed class JsonBookRepository : IRepository<Book, int>
{
    private readonly string _path;

    // Opções de serialização: camelCase, ignora nulls, indentado
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public JsonBookRepository(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path inválido", nameof(path));
        _path = path;
    }

    public Book Add(Book entity)
    {
        var list = Load();
        list.RemoveAll(b => b.Id == entity.Id); // upsert
        list.Add(entity);
        Save(list);
        return entity;
    }

    public Book? GetById(int id) => Load().FirstOrDefault(b => b.Id == id);

    public IReadOnlyList<Book> ListAll() => Load();

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

    private List<Book> Load()
    {
        if (!File.Exists(_path))
            return new List<Book>();

        var json = File.ReadAllText(_path, Encoding.UTF8);

        if (string.IsNullOrWhiteSpace(json))
            return new List<Book>();

        try
        {
            return JsonSerializer.Deserialize<List<Book>>(json, _opts) ?? new();
        }
        catch (JsonException)
        {
            // JSON inválido = lista vazia (falha gracefully)
            return new List<Book>();
        }
    }

    private void Save(List<Book> list)
    {
        var json = JsonSerializer.Serialize(list, _opts);
        File.WriteAllText(_path, json, Encoding.UTF8);
    }
}
```

---

## 3. Formato JSON

### Arquivo books.json

```json
[
  {
    "id": 1,
    "title": "Código Limpo",
    "author": "Robert C. Martin",
    "year": 2008
  },
  {
    "id": 2,
    "title": "Livro, com vírgula",
    "author": "Autor Normal",
    "year": 2020
  },
  {
    "id": 3,
    "title": "\"Clean Code\" - A Handbook",
    "author": "Uncle \"Bob\" Martin",
    "year": 2008
  },
  {
    "id": 4,
    "title": "Livro\ncom quebra\nde linha",
    "author": "Autor Normal",
    "year": 2021
  }
]
```

### Opções de Serialização

| Opção | Valor | Benefício |
|-------|-------|-----------|
| **PropertyNamingPolicy** | `CamelCase` | Segue convenção JSON (id, title, author, year) |
| **DefaultIgnoreCondition** | `WhenWritingNull` | Não serializa campos null (arquivo menor) |
| **WriteIndented** | `true` | JSON formatado para legibilidade |

---

## 4. Vantagens do JSON

| Aspecto | CSV | JSON |
|---------|-----|------|
| **Parsing** | Manual (split, escape) | Automático (System.Text.Json) |
| **Tipos** | Tudo string | Tipos nativos (int, bool, etc) |
| **Caracteres especiais** | Precisa escape (aspas, quebras) | Nativo (JSON suporta) |
| **Estrutura aninhada** | Não | Sim (arrays, objetos) |
| **Legibilidade** | Mediana | Excelente |
| **Segurança** | Precisa cuidado | Mais seguro (parsing validado) |
| **Tamanho** | Menor | Um pouco maior |

---

## 5. Uso pelo Cliente

```csharp
public static void Main()
{
    // Composição
    var jsonPath = Path.Combine(AppContext.BaseDirectory, "books.json");
    IRepository<Book, int> repo = new JsonBookRepository(jsonPath);

    // Uso (IDÊNTICO às Fases 5 e 6!)
    BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
    BookService.Register(repo, new Book(2, "DDD", "Eric Evans", 2003));

    var all = BookService.ListAll(repo);
    foreach (var book in all)
    {
        Console.WriteLine($"#{book.Id} - {book.Title} ({book.Author})");
    }
}
```

**Observação:** O cliente NÃO muda! Polimorfismo em ação!

---

## 6. Decisões de Design

### Opção 1: CamelCase
- ✅ Padrão em JSON/JavaScript
- ✅ Mais conciso (menos bytes)
- ✅ Melhor integração com APIs REST
- ❌ C# usa PascalCase por padrão

**Decisão:** Usar CamelCase (padrão JSON)

### Opção 2: Ignorar Nulls
- ✅ Arquivo menor
- ✅ Mais legível (sem campos vazios)
- ❌ Impossível distinguir "null" de "campo ausente"

**Decisão:** Ignorar nulls (BookService valida antes)

### Opção 3: Tratamento de Erro
- Load() retorna lista vazia se JSON inválido (graceful)
- Program.cs gera arquivo novo automaticamente (inicializa vazio)

---

## 7. Testes de Integração

### Cobertura (13 testes)

✅ **Arquivo e Estrutura:**
- Arquivo não existe → lista vazia
- Arquivo vazio → lista vazia
- JSON inválido → lista vazia (graceful)

✅ **Operações CRUD:**
- Add cria arquivo e persiste
- GetById encontra por ID
- Update modifica e persiste
- Remove deleta e persiste
- Operações em arquivo inexistente retornam apropriado

✅ **Caracteres Especiais:**
- Vírgula em título → preservada
- Aspas em autor → preservadas
- Quebra de linha → preservada
- Múltiplos livros combinados

### Exemplo de Teste

```csharp
[Fact]
public void Add_WithCommaInTitle_ShouldSerializeCorrectly()
{
    var path = CreateTempPath();
    var repo = new JsonBookRepository(path);

    repo.Add(new Book(1, "Livro, com vírgula", "Autor", 2020));
    var found = repo.GetById(1);

    Assert.NotNull(found);
    Assert.Equal("Livro, com vírgula", found.Title);
}
```

---

## 8. Comparação: CSV vs. JSON

| Aspecto | CSV (Fase 6) | JSON (Fase 7) |
|---------|-------------|--------------|
| **Parsing** | Manual robusto | Automático System.Text.Json |
| **Caracteres especiais** | Escape manual | Nativo |
| **Quebras de linha** | Complexo (quoted fields) | Simples (\n) |
| **Tipagem** | Tudo string | Tipos nativos |
| **Legibilidade** | Mediana | Excelente |
| **Performance** | Rápido | Muito rápido (menos processamento) |
| **Espaço** | Menor | Um pouco maior |

---

## 9. Como Executar

```powershell
cd c:\Projects\Interfaces\src\fase-07-repository-json
dotnet build
dotnet run
```

### Saída Esperada

```
=== Fase 7: Repository JSON ===

--- Operações Básicas (Persistência em JSON) ---
✓ 3 livros cadastrados em JSON

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
  #3 - Livro
com quebra
de linha (Autor Normal)

✓ Recuperado do JSON: C# com Padrões: Herança, Interfaces, etc
✓ Contém vírgula: True

--- Persistência em Arquivo ---

✓ Arquivo JSON criado: books.json
✓ Tamanho: XXX bytes

Conteúdo do JSON:
  [
    {
      "id": 1,
      "title": "C# com Padrões: Herança, Interfaces, etc",
      ...

=== Executando Testes de Integração (JSON) ===

✓ Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty
✓ Test_ListAll_WhenFileEmpty_ShouldReturnEmpty
✓ Test_Add_Then_ListAll_ShouldPersistInFile
✓ Test_GetById_Existing_ShouldReturnBook
✓ Test_GetById_Missing_ShouldReturnNull
✓ Test_Update_Existing_ShouldPersistChanges
✓ Test_Update_Missing_ShouldReturnFalse
✓ Test_Remove_Existing_ShouldDeleteFromFile
✓ Test_Remove_Missing_ShouldReturnFalse
✓ Test_Add_WithCommaInTitle_ShouldSerializeCorrectly
✓ Test_Add_WithQuotesInAuthor_ShouldSerializeCorrectly
✓ Test_Add_WithNewlineInField_ShouldSerializeCorrectly
✓ Test_Add_MultipleBooks_ShouldPersistAllAndPersist

✅ Todos os testes de integração passaram!
```

---

## 10. Limitações

- ❌ Sem concorrência (múltiplas instâncias não sincronizam)
- ❌ Load completo (toda leitura carrega arquivo inteiro)
- ❌ Sem índices (GetById faz busca linear)
- ❌ Sem versionamento de schema

---

## 11. Próximos Passos

- **Fase 8:** Banco de dados relacional (SQL Server, PostgreSQL)
- **Fase 9:** Async/await para I/O não-bloqueante
- **Fase 10:** Indexação e queries complexas
- **Fase 11:** Migrations e versionamento de schema

---

## Referências

- [System.Text.Json Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.text.json)
- [JsonSerializerOptions](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions)
- [JSON RFC 8259](https://tools.ietf.org/html/rfc8259)
- [JSON vs CSV](https://www.json.org/json-en.html)
