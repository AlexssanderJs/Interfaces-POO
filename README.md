# Interfaces em C# — Projeto Educacional de OO, Design Patterns e Testes

## 👥 Composição da Equipe

| Nome | RA |
|------|-----|
| Alexandre Filipe | 2715970 |
| Alexssander Jose | 2652480 |
| Rodrigo Alban | 2353979 |

---

## 📚 Estrutura do Projeto

O projeto segue uma progressão pedagógica de **11 fases** consolidando conceitos de programação orientada a objetos, padrões de design, princípios SOLID, e testes automatizados em C#.

### Fases de Aquecimento (0-2)
| Fase | Tema | Descrição |
|------|------|-----------|
| [Fase 0](src/fase-00-aquecimento/fase0.md) | Aquecimento | Conceitos básicos e nivelamento |
| [Fase 1](src/fase-01-procedural/fase01.md) | Procedural | Funções e estrutura imperativa |
| [Fase 2](src/fase-02-oo-sem-interface/fase02.md) | OO sem Interface | Classes e herança básica |

### Fases Principais: OO, Interfaces e Padrões (3-9)
| Fase | Tema | Conceitos-chave |
|------|------|-----------------|
| [Fase 3](src/fase-03-oo-com-heranca/fase03.md) | OO com Herança e Polimorfismo | Herança, `abstract`, polimorfismo por tipo |
| [Fase 4](src/fase-04-com-interfaces/fase04.md) | Interface Plugável e Testável | Interfaces, DIP, injeção de dependência, dublês |
| [Fase 5](src/fase-05-repository-inmemory/fase05.md) | Repository InMemory | Padrão Repository, abstração de dados |
| [Fase 6](src/fase-06-repository-csv/fase06.md) | Repository CSV | Persistência em arquivo, escape de dados |
| [Fase 7](src/fase-07-repository-json/fase07.md) | Repository JSON | Serialização JSON, tratamento de formato |
| [Fase 8](src/fase-08-ISP/) | ISP (Interface Segregation Principle) | Contratos pequenos e focados |
| [Fase 9](src/fase-09-dubles-async/fase09.md) | Dublês Assíncronos | Async/await, testes com operações assíncronas |

### Fases de Consolidação (10-11)
| Fase | Tema | Objetivo |
|------|------|----------|
| [Fase 10](src/fase-10-cheiros-antidotos/fase10.md) | Cheiros e Antídotos | Identificar e refatorar código com problemas de design |
| [Fase 11](src/fase-11-mini-projeto/README.md) | Mini-Projeto de Consolidação | Sistema completo integrando todos os conceitos |

---

## 🚀 Como Executar

### Rodar um projeto específico
```bash
# Exemplo: Fase 10
dotnet run --project src/fase-10-cheiros-antidotos/Fase10-CheirosAntidotos.csproj

# Exemplo: Fase 11 (testes + demo)
dotnet run --project src/fase-11-mini-projeto/Fase11-MiniProjeto.csproj
```

### Compilar tudo
```bash
dotnet build
```

### Instruções por fase
Cada fase possui `Program.cs` e/ou `README.md` com instruções específicas. Consulte a documentação de cada pasta.

---

## 🎯 Conceitos Progressivos

```
Fase 0-2:  Conceitos básicos (variáveis, funções, classes)
    ↓
Fase 3-5:  OO com abstração (herança, interface, padrões)
    ↓
Fase 6-9:  Persistência e testes (Repository, async, dublês)
    ↓
Fase 10:   Refatoração guiada por princípios (ISP, DIP, SRP, OCP)
    ↓
Fase 11:   Mini-projeto consolidando tudo acima
```

---

## 🏗️ Decisões de Design por Fase

### Fases 3-5: Repository Pattern
- **Fase 5**: `IRepository<T,TId>` com operações CRUD em memória.
- **Fase 6-7**: Persistência em CSV/JSON mantendo contrato idêntico.
- **Lesson**: Abstração de dados desacopla cliente de implementação.

### Fase 8: Interface Segregation Principle (ISP)
- Contratos pequenos e focados, sem métodos desnecessários.
- Cliente depende apenas do que usa.

### Fase 9: Testes com Dublês e Async
- Padrão Spy, Fake, Stub para isolar lógica.
- Operações assíncronas com cancelamento e backoff.

### Fase 10: Cheiros e Antídotos (Refatorações Mínimas)
| Cheiro | Antídoto | Princípio |
|--------|----------|-----------|
| Interface gorda | Segregar em IRead/IWrite | ISP |
| Long parameter list | Policy Object | SRP |
| Decisão espalhada (if/switch) | Catálogo único | OCP |
| Downcast recorrente | Polimorfismo via interface | DIP |
| Testes lentos com I/O | Dublês + seam | Testability |

### Fase 11: Mini-Projeto Integrado
- **Contratos**: `IReadRepository<T,TId>` e `IWriteRepository<T,TId>` segregados (ISP).
- **Repository duplo**: InMemory (testes) + CSV (persistência real).
- **Serviço**: `CatalogService` centraliza validações e lógica (SRP).
- **Composição**: Constructor Injection, inversão de dependência (DIP).
- **Testes**: 12 unitários (dublés) + 4 integração (arquivo temp).

---

## 📊 Padrões e Princípios SOLID

| Padrão/Princípio | Aplicação | Fase |
|------------------|-----------|------|
| **Padrão Repository** | Abstração de dados | 5-7, 11 |
| **Injeção de Dependência** | Constructor Injection | 4, 11 |
| **Polimorfismo** | Estratégias de cobrança, formatação | 3-4, 10 |
| **S**ingle Responsibility | Serviços focados em um domínio | 11 |
| **O**pen/Closed | Extensão sem modificação (catálogo) | 10-11 |
| **L**iskov Substitution | Implementações intercambiáveis | 5-7, 11 |
| **I**nterface Segregation | Contratos pequenos | 8, 10, 11 |
| **D**ependency Inversion | Depende de abstrações | 4, 10, 11 |
| **Dublês** (Fake, Spy, Stub) | Testes isolados sem I/O | 4, 9, 11 |

---

## 📁 Estrutura de Pastas

```
Interfaces/
├── README.md                          # Este arquivo
├── src/
│   ├── fase-00-aquecimento/
│   ├── fase-01-procedural/
│   ├── fase-02-oo-sem-interface/
│   ├── fase-03-oo-com-heranca/
│   ├── fase-04-com-interfaces/
│   ├── fase-05-repository-inmemory/
│   ├── fase-06-repository-csv/
│   ├── fase-07-repository-json/
│   ├── fase-08-ISP/
│   ├── fase-09-dubles-async/
│   ├── fase-10-cheiros-antidotos/
│   └── fase-11-mini-projeto/
├── docs/
│   └── ARCHITECTURE.md                # Diagrama e fluxos da Fase 11
└── .gitignore
```

---

## 📖 Documentação Complementar

- [Arquitetura - Fase 11](docs/ARCHITECTURE.md): Diagramas de dependência, fluxos, padrões aplicados.
- Cada fase tem seu próprio `README.md` ou `fase0X.md` com detalhes específicos.

---

## ✅ Status do Projeto

- ✅ Fases 0-9: Conceitos fundamentais → avançados
- ✅ Fase 10: 5 antídotos com testes validando refatorações
- ✅ Fase 11: Mini-projeto completo (16 testes, 5 casos de uso, rubrica 24/24)
- 📝 Documentação: Estruturada com decisões de design e exemplos

---

## 🎓 Objetivos Pedagógicos Alcançados

1. ✅ **Progressão OO**: Do procedural ao polimorfismo avançado
2. ✅ **Padrões de Design**: Repository, Policy Object, Catalogs, Dublés
3. ✅ **SOLID Principles**: ISP, DIP, SRP, OCP demonstrados em refatorações
4. ✅ **Testes Automatizados**: Unitários com dublés e integração com I/O
5. ✅ **Composição**: Injeção de dependência, sem herança profunda
6. ✅ **Código Limpo**: Refatorações pequenas, cirúrgicas, com segurança
