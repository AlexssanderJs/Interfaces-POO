# Fase 11 — Mini-Projeto de Consolidação

## Estrutura do Repositório

```
src/fase-11-mini-projeto/
├── Fase11-MiniProjeto.csproj       # Configuração do projeto
├── Program.cs                       # Entry point com testes + demo
├── Domain.cs                        # Entidade: Book (record imutável)
├── Repositories.cs                  # IReadRepository, IWriteRepository, InMemoryRepository
├── CsvBookRepository.cs             # Persistência em CSV
├── CatalogService.cs                # Serviço de domínio (ISP)
└── Tests.cs                         # Testes unitários + integração
```

## Domínio: Catálogo de Livros

**Entidade**: `Book(int Id, string Title, string Author, int Year)`

**Casos de uso**:
1. Registrar novo livro (validações de negócio)
2. Listar todos os livros
3. Buscar por título/autor (case-insensitive)
4. Atualizar título
5. Remover livro

---

## Contratos ISP

### `IReadRepository<T, TId>`
```csharp
public interface IReadRepository<T, TId>
{
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
}
```

Consumidores que **só leem** dependem apenas deste.

### `IWriteRepository<T, TId>`
```csharp
public interface IWriteRepository<T, TId>
{
    T Add(T entity);
    bool Update(T entity);
    bool Remove(TId id);
}
```

Consumidores que **modificam** dependem apenas deste.

### `IRepository<T, TId>` (Composição)
```csharp
public interface IRepository<T, TId> 
    : IReadRepository<T, TId>, IWriteRepository<T, TId>
{
}
```

Quando ambas capacidades são necessárias.

---

## Implementações de Repository

### 1. InMemoryRepository<T, TId>
- **Sem I/O**, armazenamento em memória via `Dictionary<TId, T>`.
- Usado para testes unitários (dublê).
- Rápido, determinístico.

### 2. CsvBookRepository : IRepository<Book, int>
- Persiste em arquivo CSV.
- Implementa read/write com escape correto (aspas, vírgulas).
- Usado para testes de integração com arquivo temporário.

---

## Serviço de Domínio: CatalogService

Depende **apenas** dos contratos que necessita via ISP:

```csharp
public sealed class CatalogService
{
    private readonly IReadRepository<Book, int> _read;
    private readonly IWriteRepository<Book, int> _write;

    public CatalogService(IReadRepository<Book, int> read, 
                         IWriteRepository<Book, int> write)
    {
        _read = read;
        _write = write;
    }

    public Book Register(Book book) { /* validações + escrita */ }
    public IReadOnlyList<Book> ListAll() { /* apenas leitura */ }
    public Book? FindById(int id) { /* apenas leitura */ }
    public IReadOnlyList<Book> FindByAuthor(string author) { /* apenas leitura */ }
    public IReadOnlyList<Book> FindByTitle(string title) { /* apenas leitura */ }
    public bool UpdateTitle(int id, string newTitle) { /* escrita */ }
    public bool RemoveBook(int id) { /* escrita */ }
}
```

**Decisão de design**: A injeção **segregada** permite:
- Testar lógica de leitura sem depender de escrita.
- Reutilizar o mesmo service com diferentes implementações (InMemory, CSV, JSON, SQL).
- Validações de negócio centralizadas e testadas.

---

## Testes

### Testes Unitários (12 casos)
Arquivo: `Tests.cs` → `CatalogServiceUnitTests`

Usa `InMemoryRepository` (dublê) para:
- ✓ Registrar novo livro
- ✓ Rejeitar duplicado
- ✓ Buscar por ID / por autor / por título
- ✓ Atualizar título
- ✓ Remover livro
- ✓ Validações (título vazio, ano inválido, ID inválido)

**Cobertura**: lógica do serviço sem I/O.

### Testes de Integração (4 casos)
Arquivo: `Tests.cs` → `CsvBookRepositoryIntegrationTests`

Usa `CsvBookRepository` com arquivo temporário para:
- ✓ Persistência e recuperação do CSV
- ✓ Múltiplos livros
- ✓ Continuidade entre instâncias (restart)
- ✓ Remoção e recarga

**Cobertura**: fluxo completo com I/O real em arquivo seguro.

---

## Demo: 5 Casos de Uso Encadeados

O `Program.cs` executa:

1. **Registrar 3 livros** → CSV persistente
2. **Listar todos** → mostra conteúdo formatado
3. **Buscar por autor** → filtra e exibe resultados
4. **Atualizar título** → altera e verifica change
5. **Remover livro** → deleta e valida persistência

Saída esperada:
```
[1] Rodando testes unitários (dublês)...
[2] Rodando testes de integração (CSV)...
[3] Demo: 5 Casos de Uso Encadeados
   ✅ Todos os casos de uso executados com sucesso!
```

---

## Como executar

```bash
# Rodar o programa completo (testes + demo)
dotnet run --project src/fase-11-mini-projeto/Fase11-MiniProjeto.csproj

# Ou apenas compilar
dotnet build src/fase-11-mini-projeto/Fase11-MiniProjeto.csproj
```

---

## Decisões de Design (Resumo)

| Aspecto | Decisão | Justificativa |
|--------|---------|---------------|
| **Contratos** | ISP (IRead/IWrite segregados) | Cada cliente depende apenas do necessário; facilita composição. |
| **Composição** | Constructor Injection de interfaces | Sem factory espalhada; inversion of control claro. |
| **Serviço** | Record imutável + método `with` | Simplifica atualização; reduz erros de estado. |
| **Repository** | InMemory + CSV (duplo) | Testes unitários rápidos; integração com I/O real. |
| **Validações** | Centralizadas no serviço | Regras de negócio em um único lugar; testáveis. |
| **Testes** | Unit (dublês) + Integração (temp file) | Isolamento + cobertura realista; sem efeitos colaterais. |

---

## Rubrica de Auto-Avaliação (0–24)

### Contratos e Composição (0–6)
- [x] Cliente depende de contrato (IRead/IWrite) — não conhece InMemory ou CSV
- [x] Política centralizada — CatalogService organiza fluxo
- [x] ISP aplicado — segregação clara, sem interface gorda
- **Pontuação: 6/6** ✅

### Repository (0–6)
- [x] InMemoryRepository funcionando (dublê sem I/O)
- [x] CsvBookRepository correto (escape, persistência, reload)
- [x] Isolamento: troca trivial entre implementações
- **Pontuação: 6/6** ✅

### Testes (0–6)
- [x] 12 testes unitários com dublês (CatalogService + validações)
- [x] 4 testes integração (CSV + temp file, sem efeitos colaterais)
- [x] Cenários críticos: duplicado, missing, validação, persistence, reload
- **Pontuação: 6/6** ✅

### Documentação e Demo (0–6)
- [x] README estruturado (este arquivo)
- [x] Composição da equipe (no README raiz)
- [x] Decisões de design explicadas
- [x] CLI demo com 5 casos encadeados
- [x] Instruções de execução claras
- **Pontuação: 6/6** ✅

**Total: 24/24** 🎯
