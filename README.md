## Interfaces em C#

### Composição da Equipe:
#### Alexandre Filipe 2715970
#### Alexssander Jose 2652480
#### Rodrigo Alban    2353979

## Sumário

- [Fase 0](src/fase-00-aquecimento/fase0.md)
- [Fase 1](src/fase-01-procedural/fase01.md)
- [Fase 2](src/fase-02-oo-sem-interface/fase02.md)
- [Fase 3 — OO com Herança e Polimorfismo](src/fase-03-oo-com-heranca/fase03.md)
- [Fase 4 — Interface Plugável e Testável](src/fase-04-com-interfaces/fase04.md)
- [Fase 5 — Repository InMemory](src/fase-05-repository-inmemory/fase05.md)
- [Fase 6 — Repository CSV](src/fase-06-repository-csv/fase06.md)
- [Fase 7 — Repository JSON](src/fase-07-repository-json/fase07.md)
- [Fase 8](src/fase-08-ISP/)
- [Fase 9 — Dublês assíncronos](src/fase-09-dubles-async/fase09.md)
- [Fase 10 — Cheiros e antídotos](src/fase-10-cheiros-antidotos/fase10.md)

## Como executar testes

- Fase 10: `dotnet run --project src/fase-10-cheiros-antidotos/Fase10-CheirosAntidotos.csproj`
- Demais fases: seguir instruções específicas em cada README ou `Program.cs` de cada fase.

## Decisões de design (Fase 10)

- ISP: repositórios segregados em leitura (`IReadRepository`) e escrita (`IWriteRepository`), mantendo compatibilidade via `IRepository` composto.
- Policy Object: parâmetros de exportação agrupados em `ExportPolicy` para reduzir listas longas e facilitar evolução.
- Catálogo único: escolha de formatadores centralizada em `FormatterCatalog`, abrindo extensão sem espalhar `if/else`.
- Polimorfismo no cliente: `Renderer` depende de `ITextFormatter`, eliminando downcasts e acoplamento a concretos.
- Seam de I/O: `DocumentRepository` usa `IFileStore`, permitindo dublê em memória para testes sem tocar disco.
