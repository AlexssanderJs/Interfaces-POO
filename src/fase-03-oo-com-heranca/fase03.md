## Fase 3 — OO com Herança e Polimorfismo

### Objetivo
Transformar a solução da Fase 2 (procedural com classes simples) em uma hierarquia orientada a objetos baseada em **herança** e **polimorfismo**, removendo decisões explícitas do fluxo central.

---

## Exemplo de Implementação

### Hierarquia de Classes

Baseado no domínio de **cobrança com variações** (similar à Fase 2, mas agora com polimorfismo):

```
ChargingStrategyBase (classe base abstrata)
│
├── StandardChargingStrategy (sem acréscimo)
├── DefaulterPenaltyChargingStrategy (penalidade para inadimplentes)
└── ProgressiveChargingStrategy (taxa progressiva conforme valor)
```

---

## Classe Base: Define o Ritual

```csharp
public abstract class ChargingStrategyBase
{
    /// <summary>
    /// Ritual público: orquestra o fluxo comum.
    /// Sem ramificações aqui — todo "como" é delegado à subclasse.
    /// </summary>
    public double Charge(double amount, bool isDefaulter)
    {
        // Passo comum: validação
        if (amount < 0)
            throw new ArgumentException("Valor não pode ser negativo.", nameof(amount));

        // Passo variável: gancho delegado
        return Apply(amount, isDefaulter);
    }

    /// <summary>
    /// Gancho abstrato: cada subclasse implementa sua lógica variável.
    /// </summary>
    protected abstract double Apply(double amount, bool isDefaulter);
}
```

**Mudança-chave:** Não há if/switch aqui. O fluxo é linear.

---

## Estratégias Concretas

### 1. StandardChargingStrategy — Sem Acréscimo

```csharp
public sealed class StandardChargingStrategy : ChargingStrategyBase
{
    protected override double Apply(double amount, bool isDefaulter)
    {
        // Cobrança padrão: retorna o valor como está
        return amount;
    }
}
```

### 2. DefaulterPenaltyChargingStrategy — Penalidade para Inadimplentes

```csharp
public sealed class DefaulterPenaltyChargingStrategy : ChargingStrategyBase
{
    private const double DefaulterTaxRate = 0.10; // 10%

    protected override double Apply(double amount, bool isDefaulter)
    {
        if (isDefaulter)
            return amount * (1 + DefaulterTaxRate); // +10%

        return amount;
    }
}
```

### 3. ProgressiveChargingStrategy — Taxa Progressiva

```csharp
public sealed class ProgressiveChargingStrategy : ChargingStrategyBase
{
    protected override double Apply(double amount, bool isDefaulter)
    {
        // Taxa conforme faixa de valor
        double rate = amount switch
        {
            <= 100 => 0.00,    // até 100: sem taxa
            <= 500 => 0.05,    // até 500: 5%
            _ => 0.08          // acima de 500: 8%
        };

        double charged = amount * (1 + rate);

        // Acréscimo extra se inadimplente
        if (isDefaulter)
            charged *= 1.03;   // +3% extra

        return charged;
    }
}
```

**Padrão:** cada concreta é **simples, focada e verificável**.

---

## Cliente: Instanciação (Composição Local)

```csharp
public static double Render(double amount, bool isDefaulter, string mode)
{
    // Switch APENAS para composição da estratégia inicial
    // Não há ramificações no fluxo de cálculo
    ChargingStrategyBase strategy = mode switch
    {
        "standard" => new StandardChargingStrategy(),
        "penalty" => new DefaulterPenaltyChargingStrategy(),
        "progressive" => new ProgressiveChargingStrategy(),
        _ => new StandardChargingStrategy() // padrão
    };

    // Fluxo linear: delegação via polimorfismo
    return strategy.Charge(amount, isDefaulter);
}
```

**Observação:** O switch está aqui apenas para **compor a concreta inicial**. O fluxo de cálculo é totalmente linear (nenhuma ramificação). Na Fase 4, esse switch será removido via **Factory** ou **Dependency Injection**.

---

## Como o Polimorfismo Substitui Decisões

### Antes (Procedural — Fase 1/2)
```csharp
public double Charge(double amount, bool isDefaulter, string mode)
{
    double result = amount;

    if (mode == "standard")
        result = amount;
    else if (mode == "penalty")
        result = amount * (isDefaulter ? 1.10 : 1.0);
    else if (mode == "progressive")
    {
        double rate = amount <= 100 ? 0.0 : (amount <= 500 ? 0.05 : 0.08);
        result = amount * (1 + rate);
        if (isDefaulter)
            result *= 1.03;
    }

    return result; // if/switch DENTRO do fluxo
}
```

### Agora (Polimórfico — Fase 3)
```csharp
public double Charge(double amount, bool isDefaulter)
{
    if (amount < 0)
        throw new ArgumentException(...);

    return Apply(amount, isDefaulter); // delegação polimórfica
}

protected abstract double Apply(double amount, bool isDefaulter);
// Cada subclasse implementa seu "como"
```

**Ganho:** o fluxo é linear; decisões saem de dentro para fora (subclasses).

---

## Responsabilidades

| Classe | Responsabilidade |
|--------|------------------|
| **ChargingStrategyBase** | Orquestra o ritual (`Charge`); define contrato (`Apply`) |
| **StandardChargingStrategy** | Implementa cobrança sem acréscimo |
| **DefaulterPenaltyChargingStrategy** | Implementa penalidade para inadimplentes |
| **ProgressiveChargingStrategy** | Implementa taxa progressiva conforme valor |
| **Program (Cliente)** | Instancia a estratégia concreta; chama o fluxo |

---

## O Que Melhorou ✅

1. **Remoção de if/switch no fluxo central**
   - Antes: múltiplas ramificações na função procedural a cada chamada.
   - Agora: fluxo linear na classe base; variações nas subclasses.

2. **Coesão por variação**
   - Cada estratégia trata apenas seu "como".
   - Mudança em uma não afeta as outras.

3. **Testes focados e pequenos**
   - Cada estratégia testável isoladamente.
   - Sem lógica cruzada para desembaraçar.

---

## O Que Ainda Ficou Rígido ❌

1. **Cliente conhece concretos**
   - Trocar/adicionar estratégia exige alteração no `Render`.
   - Violação do princípio Open/Closed (OCP).

2. **Composição dispersa**
   - A decisão de seleção não é centralizada nem configurável.
   - Switch permanece no cliente (será movido na Fase 4).

3. **Sem contrato estável (interface)**
   - Testes com mocks são frágeis sem uma interface explícita.
   - A Fase 4 introduzirá `IChargingStrategy` para endereçar isso.

---

## Exemplo de Execução

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

## Critérios de Avaliação

| Aspecto | Pontos |
|---------|--------|
| Hierarquia clara (base + concretas) | 0–3 |
| Substituição convincente de decisões por polimorfismo | 0–3 |
| Análise "melhorou vs. rígido" | 0–2 |
| README atualizado (equipe, sumário, execução) | 0–2 |
| **Total** | **0–10** |

---

## Próximas Fases

- **Fase 4 — OO com Interface e Factory:** Remove conhecimento de concretos via interface e centraliza composição.
- **Fase 5 — SOLID Principles:** Aprofunda aplicação de princípios de design.
