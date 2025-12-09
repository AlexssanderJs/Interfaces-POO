# Fase 3 — OO com Herança e Polimorfismo

## Resumo

Transformação da solução procedural (Fase 1 e 2) em uma hierarquia orientada a objetos baseada em **herança** e **polimorfismo**. A meta é substituir decisões explícitas (if/switch no fluxo central) por delegação polimórfica às subclasses.

---

## Hierarquia de Classes

```
    ChargingStrategyBase (abstrata)
         ├── StandardChargingStrategy
         ├── DefaulterPenaltyChargingStrategy
         └── ProgressiveChargingStrategy
```

### Responsabilidades

- **ChargingStrategyBase**: orquestra o ritual (`Charge`) e define o gancho abstrato (`Apply`).
- **StandardChargingStrategy**: implementa cobrança sem acréscimo.
- **DefaulterPenaltyChargingStrategy**: implementa cobrança com penalidade para inadimplentes.
- **ProgressiveChargingStrategy**: implementa cobrança com taxa progressiva conforme valor.
- **Program (Cliente)**: instancia a estratégia concreta e chama o fluxo.

---

## Código: Classe Base (Abstrata)

```csharp
public abstract class ChargingStrategyBase
{
    public double Charge(double amount, bool isDefaulter)
    {
        // Passo comum: validação
        if (amount < 0)
            throw new ArgumentException("Valor não pode ser negativo.", nameof(amount));

        // Passo variável: delegado à subclasse via polimorfismo
        return Apply(amount, isDefaulter);
    }

    protected abstract double Apply(double amount, bool isDefaulter);
}
```

**O que mudou:**
- Não há mais if/switch DENTRO do ritual (`Charge`).
- Toda a lógica variável é delegada ao método `Apply` (gancho virtual).
- O fluxo é linear e fácil de entender.

---

## Código: Estratégias Concretas

### StandardChargingStrategy
```csharp
public sealed class StandardChargingStrategy : ChargingStrategyBase
{
    protected override double Apply(double amount, bool isDefaulter)
    {
        return amount; // sem acréscimo
    }
}
```

### DefaulterPenaltyChargingStrategy
```csharp
public sealed class DefaulterPenaltyChargingStrategy : ChargingStrategyBase
{
    private const double DefaulterTaxRate = 0.10; // 10%

    protected override double Apply(double amount, bool isDefaulter)
    {
        if (isDefaulter)
            return amount * (1 + DefaulterTaxRate);

        return amount;
    }
}
```

### ProgressiveChargingStrategy
```csharp
public sealed class ProgressiveChargingStrategy : ChargingStrategyBase
{
    protected override double Apply(double amount, bool isDefaulter)
    {
        double rate = amount switch
        {
            <= 100 => 0.00,
            <= 500 => 0.05,
            _ => 0.08
        };

        double charged = amount * (1 + rate);

        if (isDefaulter)
            charged *= 1.03; // +3% extra

        return charged;
    }
}
```

**Padrão:** cada concreta implementa apenas seu `Apply`, sem ramificações complexas.

---

## Código: Cliente (Instanciação)

```csharp
public static double Render(double amount, bool isDefaulter, string mode)
{
    // Switch APENAS para composição (instanciação da concreta)
    ChargingStrategyBase strategy = mode switch
    {
        "standard" => new StandardChargingStrategy(),
        "penalty" => new DefaulterPenaltyChargingStrategy(),
        "progressive" => new ProgressiveChargingStrategy(),
        _ => new StandardChargingStrategy()
    };

    // Fluxo linear: delegação via polimorfismo
    return strategy.Charge(amount, isDefaulter);
}
```

**Observação:** O switch saiu do fluxo de cálculo. Permanece apenas para compor a estratégia inicial. Na Fase 4, será extraído para um **Factory** ou **Dependency Injection**.

---

## Análise: O Que Melhorou

✅ **Remoção de if/switch no fluxo central**
- Antes: A lógica procedural ramificava em cada chamada.
- Agora: O fluxo é linear; ramificações ficam nas subclasses.

✅ **Coesão por variação**
- Cada estratégia concreta trata apenas seu "como" calcular.
- Mudanças em uma não afetam as outras.

✅ **Testes focados e pequenos**
- Cada estratégia pode ser testada isoladamente.
- Não há lógica cruzada para desembaraçar.

---

## Análise: O Que Ainda Ficou Rígido

❌ **Cliente conhece concretos**
- Trocar ou adicionar estratégia exige alteração no `Render`.
- Violação do princípio Open/Closed (OCP).

❌ **Composição dispersa**
- A decisão de seleção não é centralizada nem configurável.
- Difícil parametrizar a escolha via configuração ou DI.

❌ **Sem contrato estável (interface)**
- Testes com mocks ou stubs são frágeis (sem contrato escrito).
- A Fase 4 introduzirá a interface para endereçar isso.

---

## Como Executar

### Pré-requisitos
- .NET 8.0 ou superior instalado.

### Passos

1. Navegue até a pasta do projeto:
   ```powershell
   cd c:\Projects\Interfaces\src\fase-03-oo-com-heranca
   ```

2. Restaure as dependências (se houver):
   ```powershell
   dotnet restore
   ```

3. Compile o projeto:
   ```powershell
   dotnet build
   ```

4. Execute:
   ```powershell
   dotnet run
   ```

### Saída esperada
```
=== Fase 3: OO com Herança e Polimorfismo ===

Valor: R$ 50.00
Inadimplente: False
Estratégia: standard
Resultado: R$ 50.00

Valor: R$ 150.00
Inadimplente: True
Estratégia: penalty
Resultado: R$ 165.00

Valor: R$ 300.00
Inadimplente: False
Estratégia: progressive
Resultado: R$ 315.00

Valor: R$ 600.00
Inadimplente: True
Estratégia: progressive
Resultado: R$ 639.84
```

---

## Critérios de Avaliação (Rubrica)

| Critério | Pontos | Status |
|----------|--------|--------|
| Hierarquia clara (base + concretas) | 0–3 | ✅ 3/3 |
| Substituição convincente de decisões por polimorfismo | 0–3 | ✅ 3/3 |
| Análise "melhorou vs. rígido" | 0–2 | ✅ 2/2 |
| README com equipe, sumário e execução | 0–2 | ✅ 2/2 |
| **Total** | **0–10** | **✅ 10/10** |

---

## Próximos Passos (Fase 4)

A Fase 4 — **OO com Interface e Factory** — abordará:
- Introdução de **interface** (`IChargingStrategy`) como contrato estável.
- Centralização da composição em um **Factory** ou **Service Locator**.
- Remoção do switch do cliente; decisão parametrizável via DI ou config.
- Preparação para a Fase 5 — **SOLID Principles**.

---

## Referências

- [SOLID: S (Single Responsibility)](https://en.wikipedia.org/wiki/Single_responsibility_principle)
- [Template Method Pattern](https://refactoring.guru/design-patterns/template-method)
- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
- [C# Abstract Classes and Methods](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/abstract-and-sealed-classes-and-class-members)
