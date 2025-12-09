# Fase 10 — Cheiros e antídotos (refatorações guiadas por princípios)

Cada item traz: descrição do cheiro no nosso contexto, trecho **antes**, trecho **depois**, antídoto/princípio e o teste que prova segurança do comportamento. Os códigos "depois" estão nesta pasta e executados via `dotnet run --project src/fase-10-cheiros-antidotos/Fase10-CheirosAntidotos.csproj`.

---

## 1) Interface gorda (ISP)
- **Cheiro**: `IRepository<T,TId>` (fases 5–7) obriga qualquer consumidor a depender de operações de escrita mesmo quando só lê.
- **Antes (fases 5–7)**
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
- **Depois (fase 10)**
```csharp
public interface IReadRepository<T, TId>
{
    T? GetById(TId id);
    IReadOnlyList<T> ListAll();
}
public interface IWriteRepository<T, TId>
{
    T Add(T entity);
    bool Update(T entity);
    bool Remove(TId id);
}
public interface IRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId> { }
```
- **Antídoto**: Segregação de Interface (ISP) + DIP. Consumidores só recebem as capacidades de que precisam (`ReadOnlyReportService` aceita `IReadRepository`).
- **Teste**: `TestIspSplitInterfaces` confirma que um fake somente de leitura atende ao cliente sem exigir operações de escrita.

---

## 2) Long Parameter List → Policy Object
- **Cheiro**: método com 6 parâmetros booleanos/strings para exportar relatórios.
- **Antes (versão legada)**
```csharp
string ExportLegacy(string path, bool zip, int level, bool async, string mode, string locale)
    => Export(path, zip, level, async, mode, locale);
```
- **Depois (fase 10)**
```csharp
public sealed record ExportPolicy(bool Zip, int Level, bool Async, string Mode, string Locale);
public string Export(string path, ExportPolicy policy)
    => $"path={path};zip={policy.Zip};level={policy.Level};async={policy.Async};mode={policy.Mode};locale={policy.Locale}";
```
- **Antídoto**: Policy Object (VO) reduz acoplamento e agrupa parâmetros coesos. Princípios: SRP/OCP.
- **Teste**: `TestPolicyObjectExport` compara o resultado do método novo com o wrapper legado, provando compatibilidade comportamental.

---

## 3) Decisão espalhada (if/switch duplicado)
- **Cheiro**: múltiplos `if/else` escolhendo formatadores em clientes diferentes.
- **Antes (exemplo típico)**
```csharp
if (mode == "UPPER") Console.WriteLine(text.ToUpper());
else if (mode == "lower") Console.WriteLine(text.ToLower());
else Console.WriteLine(text);
```
- **Depois (fase 10)**
```csharp
public static class FormatterCatalog
{
    private static readonly Dictionary<string, Func<ITextFormatter>> Catalog = new() {
        ["upper"] = () => new UpperCaseFormatter(),
        ["lower"] = () => new LowerCaseFormatter(),
        ["raw"]   = () => new PassthroughFormatter()
    };
    public static ITextFormatter Resolve(string mode)
        => string.IsNullOrWhiteSpace(mode)
           ? new PassthroughFormatter()
           : Catalog.TryGetValue(mode.ToLowerInvariant(), out var f) ? f() : new PassthroughFormatter();
}
```
- **Antídoto**: Ponto único de composição/catalogação. Princípios: OCP (novos modos só adicionam ao catálogo) e DIP (cliente recebe contrato `ITextFormatter`).
- **Teste**: `TestCatalogCentralizesDecision` cobre três políticas (upper/lower/fallback) sem espalhar decisões.

---

## 4) Downcast/pattern matching no cliente
- **Cheiro**: cliente recebia `object` e fazia `is` para cada formatador, crescendo para cada nova variação.
- **Antes**
```csharp
void Render(object fmt, string s)
{
    if (fmt is UpperCaseFormatter u) Console.WriteLine(u.Apply(s));
    else if (fmt is LowerCaseFormatter l) Console.WriteLine(l.Apply(s));
}
```
- **Depois (fase 10)**
```csharp
public sealed class Renderer
{
    public string Render(ITextFormatter formatter, string text)
        => formatter.Apply(text);
}
```
- **Antídoto**: Polimorfismo via contrato específico (`ITextFormatter`). Princípios: DIP + LSP (qualquer formatter serve).
- **Teste**: `TestRendererUsesPolymorphism` injeta um `FakeFormatter` e garante chamada única sem casts.

---

## 5) Teste lento com I/O em disco
- **Cheiro**: repositórios CSV/JSON nas fases 6–7 usavam `File.ReadAllText/WriteAllText` nos testes de unidade.
- **Antes (trecho CSV)**
```csharp
File.WriteAllText(_path, sb.ToString(), Encoding.UTF8);
```
- **Depois (fase 10)**
```csharp
public interface IFileStore { bool Exists(string path); string Read(string path); void Write(string path, string content); }
public sealed class DocumentRepository
{
    public DocumentRepository(IFileStore store) { _store = store; }
    public void Save(Document doc, string path) => _store.Write(path, $"{doc.Id}|{doc.Content}");
    public Document? Load(string path) => !_store.Exists(path) ? null : Parse(_store.Read(path));
}
public sealed class InMemoryFileStore : IFileStore { /* guarda em memória */ }
```
- **Antídoto**: Dublê + seam de I/O. Princípios: DIP (abstrair I/O), Testability. Testes rodam sem tocar disco.
- **Teste**: `TestInMemoryFileStoreAvoidsDisk` persiste documento em `InMemoryFileStore`, verifica roundtrip e que o caminho não existe no disco.

---

## Como rodar
```
dotnet run --project src/fase-10-cheiros-antidotos/Fase10-CheirosAntidotos.csproj
```
Saída esperada: todos os testes da fase 10 passando.
