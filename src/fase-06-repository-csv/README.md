# Fase 6 — Repository CSV

## Resumo

Evolução do Repository para persistir dados em arquivo **CSV**, mantendo o mesmo contrato `IRepository<T, TId>`. Implementa escape correto de vírgulas, aspas e quebras de linha, com testes de integração completos.

---

## Estrutura

```
IRepository<T, TId>         (Contrato - mesma interface da Fase 5)
    ↓
CsvBookRepository           (Implementação CSV)
    ├── Load()              (lê arquivo CSV)
    ├── Save()              (escreve arquivo CSV)
    ├── Escape()            (trata caracteres especiais)
    └── SplitCsvLine()      (parse de CSV)
    ↓
books.csv                   (arquivo em disco)
```

---

## Arquivos

- **IRepository.cs**: Contrato reaproveitado
- **Book.cs**: Modelo de domínio
- **CsvBookRepository.cs**: Implementação CSV com Load/Save
- **BookService.cs**: Serviço de domínio
- **Program.cs**: Demonstrações de uso
- **CsvBookRepositoryTests.cs**: 13 testes de integração

---

## Características

✅ **Mesma interface:** `IRepository<Book, int>` funciona para InMemory e CSV  
✅ **Escape correto:** Vírgulas, aspas duplas, quebras de linha  
✅ **Cabeçalho CSV:** Id,Title,Author,Year  
✅ **Encoding UTF-8:** Suporta caracteres especiais  
✅ **Testes robustos:** Arquivo temporário, casos especiais  
✅ **Sem concorrência:** Lê/escreve tudo (simples e determinístico)

---

## Operações

| Operação | Comportamento |
|----------|---------------|
| Add | Insere ou substitui (upsert) se ID existe |
| GetById | Busca por ID, retorna null se não existe |
| ListAll | Lista todos, arquivo vazio = lista vazia |
| Update | Atualiza se existe, retorna false se não |
| Remove | Remove se existe, retorna false se não |

---

## Exemplo de Uso

```csharp
var csvPath = Path.Combine(AppContext.BaseDirectory, "books.csv");
IRepository<Book, int> repo = new CsvBookRepository(csvPath);

// Registrar livros
BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
BookService.Register(repo, new Book(2, "DDD", "Eric Evans", 2003));

// Listar
var all = BookService.ListAll(repo);

// Buscar
var found = BookService.FindById(repo, 1);

// Atualizar
var updated = found!.WithTitle("Novo Título");
BookService.Update(repo, updated);

// Remover
BookService.Remove(repo, 2);
```

---

## Formato CSV

```
Id,Title,Author,Year
1,Código Limpo,Robert C. Martin,2008
2,"Livro, com vírgula",Autor Normal,2020
3,"Livro ""com"" aspas",Outro,2021
```

---

## Testes (13 testes, 100% pass)

### Básicos
- ✅ Arquivo não existe → lista vazia
- ✅ Arquivo vazio → lista vazia
- ✅ Add + ListAll persiste em arquivo
- ✅ GetById existente retorna
- ✅ GetById ausente retorna null
- ✅ Update existente persiste
- ✅ Update ausente retorna false
- ✅ Remove existente deleta
- ✅ Remove ausente retorna false

### Caracteres Especiais
- ✅ Vírgula em título escapa corretamente
- ✅ Aspas em autor escapa corretamente
- ✅ Quebra de linha escapa corretamente
- ✅ Múltiplos livros com tudo junto

---

## Comparação Fase 5 vs Fase 6

| Aspecto | Fase 5 | Fase 6 |
|---------|--------|--------|
| Interface | `IRepository<T, TId>` | **Mesma** |
| Storage | RAM (List) | Arquivo CSV |
| Persistência | Perdida ao desligar | Permanente em disco |
| Performance | Muito rápido | Mais lento (I/O) |
| Uso | Testes, prototipagem | Dados simples |

**Vantagem:** Polimorfismo! Cliente não muda!

---

## Como Executar

```powershell
cd c:\Projects\Interfaces\src\fase-06-repository-csv
dotnet build
dotnet run
```

---

## Benefícios

✅ **Reutilização de contrato:** Interface igual funciona para diferentes implementations  
✅ **Persistência real:** Dados sobrevivem à execução  
✅ **Escape robusto:** Suporta caracteres especiais  
✅ **Testes de integração:** Arquivo real, cenários completos  
✅ **Determinístico:** Sem I/O assíncrono ou locks

---

## Limitações

- ❌ Sem concorrência (múltiplas instâncias não sincronizam)
- ❌ Sem índices (GetById faz busca linear)
- ❌ Sem versionamento de schema
- ❌ Performance pior que InMemory

---

## Próximos Passos

- **Fase 7:** Banco de dados SQL (SQL Server, PostgreSQL)
- **Fase 8:** Async/await para I/O não-bloqueante
- **Fase 9:** Índices e queries complexas
