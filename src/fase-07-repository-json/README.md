# Fase 7 — Repository JSON

## Resumo

Evolução do Repository para persistir dados em arquivo **JSON** usando `System.Text.Json`. Implementa serialização com camelCase e ignora nulls, com testes de integração completos.

---

## Estrutura

```
IRepository<T, TId>         (Contrato - mesma interface das Fases 5-6)
    ↓
JsonBookRepository          (Implementação JSON)
    ├── Load()              (desserializa JSON)
    ├── Save()              (serializa JSON)
    └── JsonSerializerOptions (camelCase, ignora nulls)
    ↓
books.json                  (arquivo em disco)
```

---

## Arquivos

- **IRepository.cs**: Contrato reaproveitado
- **Book.cs**: Modelo de domínio
- **JsonBookRepository.cs**: Implementação JSON com Load/Save
- **BookService.cs**: Serviço de domínio
- **Program.cs**: Demonstrações de uso
- **JsonBookRepositoryTests.cs**: 13 testes de integração

---

## Características

✅ **Mesma interface:** `IRepository<Book, int>` funciona para InMemory, CSV e JSON  
✅ **System.Text.Json:** Sem dependências externas (built-in)  
✅ **CamelCase:** Propriedades em snake_case (padrão JSON)  
✅ **Ignora nulls:** Arquivo mais compacto  
✅ **Indentado:** Legível para edição manual  
✅ **Testes robustos:** Arquivo temporário, casos especiais  
✅ **Tratamento graceful:** JSON inválido = lista vazia

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
var jsonPath = Path.Combine(AppContext.BaseDirectory, "books.json");
IRepository<Book, int> repo = new JsonBookRepository(jsonPath);

// Registrar livros
BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
BookService.Register(repo, new Book(2, "DDD", "Eric Evans", 2003));

// Listar
var all = BookService.ListAll(repo);

// Buscar
var found = BookService.FindById(repo, 1);

// Atualizar
var updated = found! with { Title = "Novo Título" };
BookService.Update(repo, updated);

// Remover
BookService.Remove(repo, 2);
```

---

## Formato JSON

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
  }
]
```

---

## Testes (13 testes, 100% pass)

### Básicos
- ✅ Arquivo não existe → lista vazia
- ✅ Arquivo vazio → lista vazia
- ✅ JSON inválido → lista vazia (graceful)
- ✅ Add + ListAll persiste em arquivo
- ✅ GetById existente retorna
- ✅ GetById ausente retorna null
- ✅ Update existente persiste
- ✅ Update ausente retorna false
- ✅ Remove existente deleta
- ✅ Remove ausente retorna false

### Caracteres Especiais
- ✅ Vírgula em título preservada
- ✅ Aspas em autor preservadas
- ✅ Quebra de linha preservada
- ✅ Múltiplos livros com tudo junto

---

## Opções de Serialização

```csharp
private static readonly JsonSerializerOptions _opts = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,      // id, title, author, year
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // não serializa null
    WriteIndented = true                                    // formatado para leitura
};
```

---

## Comparação Fase 6 vs Fase 7

| Aspecto | CSV | JSON |
|---------|-----|------|
| Interface | `IRepository<T, TId>` | **Mesma** |
| Storage | Arquivo CSV | Arquivo JSON |
| Parsing | Manual (split, escape) | Automático (System.Text.Json) |
| Caracteres especiais | Escape manual | Nativo |
| Quebras de linha | Complexo (quoted) | Simples (\n) |
| Legibilidade | Mediana | Excelente |
| Performance | Rápido | Muito rápido |
| Tamanho | Menor | Um pouco maior |

**Vantagem:** Polimorfismo! Cliente não muda!

---

## Como Executar

```powershell
cd c:\Projects\Interfaces\src\fase-07-repository-json
dotnet build
dotnet run
```

---

## Benefícios

✅ **Reutilização de contrato:** Interface igual funciona para diferentes implementations  
✅ **Sem dependências:** System.Text.Json é built-in no .NET  
✅ **Serialização robusta:** Automática e validada  
✅ **Testes de integração:** Arquivo real, cenários completos  
✅ **Determinístico:** Sem I/O assíncrono

---

## Limitações

- ❌ Sem concorrência (múltiplas instâncias não sincronizam)
- ❌ Sem índices (GetById faz busca linear)
- ❌ Sem versionamento de schema
- ❌ Performance pior que InMemory

---

## Próximos Passos

- **Fase 8:** Banco de dados SQL (SQL Server, PostgreSQL)
- **Fase 9:** Async/await para I/O não-bloqueante
- **Fase 10:** Índices e queries complexas
