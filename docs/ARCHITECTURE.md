# Arquitetura - Fase 11

## Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────┐
│                     Program.cs (CLI)                     │
│  - Orquestra testes unitários                            │
│  - Executa demo com 5 casos de uso                       │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ├─ CatalogServiceUnitTests (dublês)
                   └─ CsvBookRepositoryIntegrationTests (I/O)
                   
                        │
        ┌───────────────┴───────────────┐
        │                               │
   ┌────▼──────────────────────┐   ┌────▼──────────────────┐
   │   CatalogService          │   │    (Tests)             │
   │  (Serviço de Domínio)     │   └────────────────────────┘
   │                            │
   │  - Register(book)         │
   │  - ListAll()              │
   │  - FindById()             │
   │  - FindByAuthor()         │
   │  - FindByTitle()          │
   │  - UpdateTitle()          │
   │  - RemoveBook()           │
   └────┬─────────────┬────────┘
        │             │
   (depende de ISP)   │
        │             │
   ┌────▼──────────┐ ┌▼──────────────┐
   │ IReadRepo     │ │ IWriteRepo    │
   │  - GetById    │ │  - Add        │
   │  - ListAll    │ │  - Update     │
   │              │ │  - Remove     │
   └────┬──────────┘ └┬──────────────┘
        │            │
   ┌────┴────────┬───┴──────────┐
   │             │              │
   │     (implementações)        │
   │             │              │
┌──▼────────────┐│┌────────────▼──┐
│ InMemory      ││  CsvBook       │
│ Repository   ││  Repository    │
│ (Dublê)      ││  (Persistência)│
│              ││                │
│ Dict<TId,T>  ││ File I/O CSV   │
└──────────────┘└────────────────┘
       │                │
  (testes)        (testes)
  unitários       integração
```

## Fluxo de Dependência (Inversão)

```
Camada de Apresentação (Program.cs)
    ↓
Serviço de Domínio (CatalogService)
    ↓
Contratos ISP (IReadRepository, IWriteRepository)
    ↓
Implementações Concretas (InMemory, Csv)

Note: CatalogService NÃO depende de InMemory ou Csv.
Implementações são "plugáveis" via constructor injection.
```

## Isolamento de Responsabilidades

| Componente | Responsabilidade | Testabilidade |
|-----------|-----------------|---------------|
| **Domain.cs** | Entidade Book (record) | Teste: imutabilidade |
| **Repositories.cs** | Contrato + InMemory | Teste: sem I/O |
| **CsvBookRepository.cs** | Persistência CSV | Teste: arquivo temp |
| **CatalogService.cs** | Lógica de negócio + validações | Teste: dublês |
| **Tests.cs** | Unit + Integração | Cobertura total |
| **Program.cs** | Orquestração + demo | Resultado visível |

---

## Padrões Aplicados

1. **Dependency Injection**: construtores recebem interfaces, não concretos.
2. **Repository Pattern**: abstrai acesso a dados (InMemory vs CSV).
3. **ISP (Interface Segregation Principle)**: IRead e IWrite separados.
4. **DIP (Dependency Inversion Principle)**: depende de abstrações, não de concretos.
5. **Record (C# 9+)**: imutabilidade + pattern matching com `with`.
6. **Composition Over Inheritance**: CatalogService compõe repos, não estende.

---

## Fluxo de um Caso de Uso (Exemplo: Registrar Livro)

```
Program.cs
  │
  └─ CatalogService.Register(book)
       │
       ├─ ValidateBook(book) ✓
       │   - ID > 0
       │   - Título não vazio
       │   - Autor não vazio
       │   - Ano válido
       │
       ├─ IReadRepository.GetById(book.Id) → null?
       │   (verifica duplicado)
       │
       └─ IWriteRepository.Add(book)
           │
           ├─ (Se InMemory) Dictionary[id] = book
           └─ (Se Csv) File.WriteAllText(path, csv)
```

---

## Mapa de Testes

```
CatalogServiceUnitTests (12 testes, sem I/O)
  ├─ TestRegisterNewBook
  ├─ TestRegisterDuplicateThrows
  ├─ TestFindById / FindByIdMissing
  ├─ TestFindByAuthor / TestFindByTitle
  ├─ TestUpdateTitle / TestUpdateTitleMissing
  ├─ TestRemoveBook / TestRemoveBookMissing
  ├─ TestValidationEmptyTitle
  └─ TestValidationInvalidYear

CsvBookRepositoryIntegrationTests (4 testes, com I/O temp)
  ├─ TestCsvPersistence
  ├─ TestCsvWithMultipleBooks
  ├─ TestCsvLoadAfterRestart
  └─ TestCsvRemoveAndReload
```

Total: **16 testes** cobrindo fluxos críticos.

---

## Tecnologias

- **Linguagem**: C# (.NET 10)
- **Padrão de Projeto**: Repository + Service
- **Testes**: Manual (sem framework externo)
- **Persistência**: CSV (sem banco de dados)
- **Injeção de Dependência**: Constructor Injection

---

Ver também: [README.md](../fase-11-mini-projeto/README.md)
