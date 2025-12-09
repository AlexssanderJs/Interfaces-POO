# Fase 5 — Repository InMemory

## Resumo

Implementação do **padrão Repository** como ponto único de acesso a dados, usando o domínio `Book` com implementação **InMemory** baseada em `Dictionary`. O cliente fala **apenas com a interface** `IRepository`, nunca com coleções diretamente.

---

## Estrutura do Projeto

```
IRepository<T, TId>           (Contrato genérico)
    ↑
    │ implementa
    │
InMemoryRepository<T, TId>    (Implementação em memória)
    ↓
Dictionary<TId, T>            (Coleção interna - nunca exposta)

BookService                   (Regras de negócio)
    ↓ usa
IRepository<Book, int>        (Depende do contrato)
```

---

## Arquivos

- **IRepository.cs**: Contrato genérico do Repository
- **InMemoryRepository.cs**: Implementação em memória com Dictionary
- **Book.cs**: Modelo de domínio (record imutável)
- **BookService.cs**: Serviço com regras de negócio
- **Program.cs**: Demonstrações de uso
- **InMemoryRepositoryTests.cs**: Testes unitários (12 testes)

---

## Operações do Repository

| Método | Descrição | Retorno |
|--------|-----------|---------|
| `Add(T)` | Adiciona entidade | `T` |
| `GetById(TId)` | Busca por ID | `T?` (null se não existir) |
| `ListAll()` | Lista todas | `IReadOnlyList<T>` |
| `Update(T)` | Atualiza existente | `bool` (false se não existir) |
| `Remove(TId)` | Remove por ID | `bool` (false se não existir) |

---

## Características

✅ **Genérico**: Funciona com qualquer tipo (`T`, `TId`)  
✅ **Sem I/O**: Apenas memória (rápido para testes)  
✅ **Encapsulamento**: Coleção interna nunca exposta  
✅ **Imutabilidade**: Retorna `IReadOnlyList<T>`  
✅ **Testável**: 12 testes cobrindo operações + fronteiras

---

## Como Usar

### 1. Criar o Repository

```csharp
IRepository<Book, int> repo = new InMemoryRepository<Book, int>(book => book.Id);
```

### 2. Adicionar Livros

```csharp
BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
BookService.Register(repo, new Book(2, "DDD", "Eric Evans", 2003));
```

### 3. Consultar

```csharp
// Buscar por ID
var book = BookService.FindById(repo, 1);

// Listar todos
var all = BookService.ListAll(repo);

// Buscar por autor
var martinBooks = BookService.FindByAuthor(repo, "Martin");
```

### 4. Atualizar e Remover

```csharp
// Atualizar
var updated = book.WithTitle("Código Limpo (2ª Edição)");
BookService.Update(repo, updated);

// Remover
BookService.Remove(repo, 1);
```

---

## Testes (12 testes, 100% pass)

### Operações Básicas
- ✅ Add + ListAll retorna 1 item
- ✅ Add múltiplos retorna todos
- ✅ GetById existente retorna entidade
- ✅ GetById ausente retorna null
- ✅ Update existente retorna true
- ✅ Update ausente retorna false
- ✅ Remove existente retorna true
- ✅ Remove ausente retorna false

### Cenários de Fronteira
- ✅ Add duplicado sobrescreve
- ✅ ListAll vazio retorna lista vazia
- ✅ Update altera dados corretamente
- ✅ ListAll retorna IReadOnlyList

---

## Executar

```powershell
cd c:\Projects\Interfaces\src\fase-05-repository-inmemory
dotnet build
dotnet run
```

---

## Benefícios do Padrão Repository

### ✅ Separação de Responsabilidades
- Repository: apenas persistência
- Service: regras de negócio
- Cliente: orquestração

### ✅ Testabilidade
- Testes rápidos (sem I/O)
- Fácil criar FakeRepository
- Cobertura completa de cenários

### ✅ Flexibilidade
- Fácil trocar implementação (SqlRepository, JsonRepository)
- Cliente não muda ao trocar implementação
- Ponto único para logging, cache, etc.

### ✅ Encapsulamento
- Coleção interna nunca exposta
- Cliente não conhece estrutura de dados
- Muda implementação sem impactar cliente

---

## Comparação

### ❌ Sem Repository
```csharp
var books = new List<Book>();  // Cliente conhece List
books.Add(new Book(...));       // Acesso direto à coleção
var found = books.FirstOrDefault(b => b.Id == 1);
```

### ✅ Com Repository
```csharp
IRepository<Book, int> repo = ...;  // Cliente conhece apenas contrato
repo.Add(new Book(...));             // Abstração
var found = repo.GetById(1);         // API semântica
```

---

## Próximos Passos

- **Fase 6**: Implementações com persistência real (JSON, SQL)
- **Fase 7**: Unit of Work (transações, coordenação)
- **Fase 8**: Specification Pattern (consultas complexas)

---

## Referências

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
