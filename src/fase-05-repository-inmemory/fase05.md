## Fase 5 — Repository InMemory (Contrato + Implementação em Coleção)

### Objetivo
Introduzir o **padrão Repository** como ponto único de acesso a dados, usando um domínio simples (`Book`) com implementação **InMemory** baseada em coleção. O cliente fala **apenas com o Repository**, nunca com coleções diretamente.

---

## Arquitetura

```
┌──────────────────────────────────────────────────────────┐
│                      Cliente                              │
│                  (BookService / Program)                  │
└────────────────────┬─────────────────────────────────────┘
                     │ depende de
                     ▼
┌──────────────────────────────────────────────────────────┐
│              IRepository<Book, int>                       │
│                   (CONTRATO)                              │
│  + Add(Book): Book                                       │
│  + GetById(int): Book?                                   │
│  + ListAll(): IReadOnlyList<Book>                       │
│  + Update(Book): bool                                    │
│  + Remove(int): bool                                     │
└────────────────────┬─────────────────────────────────────┘
                     │ implementa
                     ▼
┌──────────────────────────────────────────────────────────┐
│         InMemoryRepository<Book, int>                    │
│              (IMPLEMENTAÇÃO)                              │
│  - Dictionary<int, Book> _store                          │
│  - Func<Book, int> _getId                               │
└──────────────────────────────────────────────────────────┘
```

**Fluxo:**
```
Cliente → IRepository → InMemoryRepository → Dictionary (coleção interna)
```

**Importante:** Cliente NUNCA acessa a coleção diretamente!

---

## 1. Contrato do Repository

```csharp
public interface IRepository<T, TId>
{
    T Add(T entity);
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
    bool Update(T entity);
    bool Remove(TId id);
}
```

**Características:**
- ✅ Genérico: reutilizável para qualquer domínio
- ✅ Expõe apenas operações de acesso a dados
- ✅ Retorna `IReadOnlyList<T>` (não expõe coleções mutáveis)
- ✅ SEM regras de negócio (apenas persistência)

---

## 2. Implementação InMemory

```csharp
public sealed class InMemoryRepository<T, TId> : IRepository<T, TId>
    where TId : notnull
{
    private readonly Dictionary<TId, T> _store = new();
    private readonly Func<T, TId> _getId;

    public InMemoryRepository(Func<T, TId> getId)
    {
        _getId = getId ?? throw new ArgumentNullException(nameof(getId));
    }

    public T Add(T entity)
    {
        var id = _getId(entity);
        _store[id] = entity; // adiciona ou substitui (upsert)
        return entity;
    }

    public T? GetById(TId id)
    {
        return _store.TryGetValue(id, out var entity) ? entity : default;
    }

    public IReadOnlyList<T> ListAll()
    {
        return _store.Values.ToList(); // lista somente leitura
    }

    public bool Update(T entity)
    {
        var id = _getId(entity);
        if (!_store.ContainsKey(id))
            return false;

        _store[id] = entity;
        return true;
    }

    public bool Remove(TId id)
    {
        return _store.Remove(id);
    }
}
```

**Características:**
- ✅ **Sem I/O:** apenas memória (Dictionary)
- ✅ **Rápido:** ideal para testes e prototipagem
- ✅ **Política de ID:** extrai o ID da entidade via `Func<T, TId>`
- ✅ **Genérico:** funciona com qualquer tipo

---

## 3. Modelo de Domínio

```csharp
public sealed record Book(int Id, string Title, string Author, int Year)
{
    public Book WithTitle(string newTitle) => this with { Title = newTitle };
    public Book WithAuthor(string newAuthor) => this with { Author = newAuthor };
    public Book WithYear(int newYear) => this with { Year = newYear };
}
```

**Por que `record`?**
- ✅ Imutabilidade por padrão
- ✅ Igualdade por valor (não por referência)
- ✅ Sintaxe `with` para criar cópias modificadas

---

## 4. Serviço de Domínio

```csharp
public static class BookService
{
    public static Book Register(IRepository<Book, int> repo, Book book)
    {
        ValidateBook(book); // validações de negócio

        var existing = repo.GetById(book.Id);
        if (existing != null)
            throw new InvalidOperationException($"Livro com ID {book.Id} já existe.");

        return repo.Add(book);
    }

    public static IReadOnlyList<Book> ListAll(IRepository<Book, int> repo)
    {
        return repo.ListAll();
    }

    public static Book? FindById(IRepository<Book, int> repo, int id)
    {
        return repo.GetById(id);
    }

    public static bool Update(IRepository<Book, int> repo, Book book)
    {
        ValidateBook(book);
        return repo.Update(book);
    }

    public static bool Remove(IRepository<Book, int> repo, int id)
    {
        return repo.Remove(id);
    }

    public static IReadOnlyList<Book> FindByAuthor(IRepository<Book, int> repo, string author)
    {
        return repo.ListAll()
                   .Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase))
                   .ToList();
    }

    private static void ValidateBook(Book book)
    {
        if (book == null)
            throw new ArgumentNullException(nameof(book));
        if (book.Id <= 0)
            throw new ArgumentException("ID deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(book.Title))
            throw new ArgumentException("Título é obrigatório.");
        if (string.IsNullOrWhiteSpace(book.Author))
            throw new ArgumentException("Autor é obrigatório.");
        if (book.Year < 1000 || book.Year > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano inválido.");
    }
}
```

**Separação de Responsabilidades:**
- **BookService:** Validações e regras de negócio
- **IRepository:** Apenas persistência (sem lógica de negócio)

---

## 5. Uso pelo Cliente

```csharp
public static void Main()
{
    // Composição: cria o repositório
    IRepository<Book, int> repo = new InMemoryRepository<Book, int>(book => book.Id);

    // Cliente fala APENAS com o Repository via BookService
    BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
    BookService.Register(repo, new Book(2, "Domain-Driven Design", "Eric Evans", 2003));

    // Listar todos
    var all = BookService.ListAll(repo);
    foreach (var book in all)
    {
        Console.WriteLine($"#{book.Id} - {book.Title} ({book.Author}, {book.Year})");
    }

    // Buscar por ID
    var found = BookService.FindById(repo, 2);
    Console.WriteLine($"Encontrado: {found?.Title}");

    // Atualizar
    var updated = found!.WithTitle("Domain-Driven Design (Edição Revisada)");
    BookService.Update(repo, updated);

    // Remover
    BookService.Remove(repo, 1);
}
```

**Observação:** Cliente NUNCA acessa `_store` diretamente, apenas via `IRepository`.

---

## 6. Testes Unitários (Sem I/O)

### Cobertura de Testes

✅ **Operações Básicas:**
- `Add` + `ListAll` → retorna 1 item
- `GetById` existente → retorna entidade
- `GetById` ausente → retorna `null`
- `Update` existente → retorna `true`
- `Update` ausente → retorna `false`
- `Remove` existente → retorna `true`
- `Remove` ausente → retorna `false`

✅ **Cenários de Fronteira:**
- `Add` duplicado → sobrescreve
- `ListAll` vazio → lista vazia
- `Update` altera dados corretamente
- `ListAll` retorna `IReadOnlyList`

### Exemplo de Teste

```csharp
private static void Test_Add_Then_ListAll_ShouldReturnOneItem()
{
    var repo = new InMemoryRepository<Book, int>(b => b.Id);
    repo.Add(new Book(1, "Livro A", "Autor X", 2020));

    var all = repo.ListAll();

    Assert(all.Count == 1, "Deveria ter 1 item");
    Assert(all[0].Id == 1, "ID deveria ser 1");
    Console.WriteLine("✓ Teste passou");
}
```

**Benefícios:**
- ✅ **Sem I/O:** testes rápidos (milissegundos)
- ✅ **Determinísticos:** sempre mesmo resultado
- ✅ **Isolados:** cada teste cria seu próprio repositório
- ✅ **Focados:** testam apenas o Repository, não o BD

---

## 7. Comparação com Acesso Direto a Coleções

### ❌ Sem Repository (acoplamento alto)
```csharp
// Cliente acessa coleção diretamente
var books = new List<Book>();
books.Add(new Book(1, "Livro A", "Autor", 2020));
var found = books.FirstOrDefault(b => b.Id == 1);

// Problemas:
// - Cliente conhece estrutura interna (List)
// - Difícil trocar para BD sem reescrever cliente
// - Sem ponto único para logging/cache/validação
// - Testes acoplados à implementação
```

### ✅ Com Repository (acoplamento baixo)
```csharp
// Cliente acessa via contrato
IRepository<Book, int> repo = new InMemoryRepository<Book, int>(b => b.Id);
repo.Add(new Book(1, "Livro A", "Autor", 2020));
var found = repo.GetById(1);

// Benefícios:
// - Cliente depende do contrato, não da implementação
// - Fácil trocar para SqlRepository sem mudar cliente
// - Ponto único para cross-cutting concerns
// - Testes desacoplados (pode usar FakeRepository)
```

---

## 8. Como Executar

### Pré-requisitos
- .NET 10.0 ou superior

### Comandos

```powershell
# Navegar até a pasta
cd c:\Projects\Interfaces\src\fase-05-repository-inmemory

# Compilar
dotnet build

# Executar (inclui demonstrações + testes)
dotnet run
```

### Saída Esperada

```
=== Fase 5: Repository InMemory ===

--- Operações Básicas ---
✓ 4 livros cadastrados

Livros cadastrados:
  #1 - Código Limpo (Robert C. Martin, 2008)
  #2 - Domain-Driven Design (Eric Evans, 2003)
  #3 - Refactoring (Martin Fowler, 1999)
  #4 - Design Patterns (Gang of Four, 1994)

✓ Busca por ID 2: Domain-Driven Design
✓ Livro ID 2 atualizado: Domain-Driven Design (Edição Revisada)
✓ Livro ID 4 removido. Total: 3 livros

--- Consultas Personalizadas ---
...

--- Cenários de Fronteira ---
...

=== Executando Testes do Repository ===

✓ Test_Add_Then_ListAll_ShouldReturnOneItem
✓ Test_Add_Multiple_ShouldReturnAll
✓ Test_GetById_Existing_ShouldReturnEntity
✓ Test_GetById_Missing_ShouldReturnNull
✓ Test_Update_Existing_ShouldReturnTrue
✓ Test_Update_Missing_ShouldReturnFalse
✓ Test_Remove_Existing_ShouldReturnTrue
✓ Test_Remove_Missing_ShouldReturnFalse
✓ Test_Add_Duplicate_ShouldOverwrite
✓ Test_ListAll_Empty_ShouldReturnEmptyList
✓ Test_Update_ChangesData
✓ Test_ListAll_ReturnsReadOnlyList

✅ Todos os testes passaram!
```

---

## 9. O Que Melhorou ✅

### Separação de Responsabilidades
- **Repository:** apenas persistência (sem lógica de negócio)
- **Service:** regras de negócio e validações
- **Cliente:** orquestração e fluxo

### Testabilidade
- Testes sem I/O (rápidos e determinísticos)
- Fácil criar `FakeRepository` para testes do Service
- Cobertura completa de cenários (happy path + edge cases)

### Flexibilidade
- Fácil trocar implementação (`SqlRepository`, `JsonRepository`)
- Cliente não muda ao trocar implementação
- Ponto único para cross-cutting concerns (log, cache, etc.)

### Manutenibilidade
- Lógica de acesso a dados centralizada
- Cliente simples (fala apenas com o contrato)
- Fácil adicionar novos repositórios para outros domínios

---
