## Fase 4 — Interface Plugável e Testável (Contrato + Composição + Dublês)

### Objetivo
Evoluir a solução da Fase 3 introduzindo:
1. **Contrato explícito** (interface) para o passo variável
2. **Ponto único de composição** (Factory/Catálogo) - política → implementação
3. **Cliente dependendo apenas do contrato** - não conhece concretos
4. **Testes com dublês** (fake/stub) - sem I/O, rápidos e determinísticos

---

## Estrutura Conceitual

### 1. Contrato (Interface)
Descreve o **"o que"** a peça faz (comportamento esperado), não o **"como"**.

```csharp
public interface IChargingStrategy
{
    double Calculate(double amount, bool isDefaulter);
}
```

**Características:**
- Assinatura enxuta
- Foco no comportamento variável
- Cliente depende dela, não de concretos

---

### 2. Implementações Concretas
Cada classe `sealed` implementa o contrato com comportamento específico.

#### StandardChargingStrategy
```csharp
public sealed class StandardChargingStrategy : IChargingStrategy
{
    public double Calculate(double amount, bool isDefaulter)
    {
        return amount; // sem acréscimo
    }
}
```

#### DefaulterPenaltyChargingStrategy
```csharp
public sealed class DefaulterPenaltyChargingStrategy : IChargingStrategy
{
    private const double PenaltyRate = 0.10;

    public double Calculate(double amount, bool isDefaulter)
    {
        return isDefaulter ? amount * (1 + PenaltyRate) : amount;
    }
}
```

#### ProgressiveChargingStrategy
```csharp
public sealed class ProgressiveChargingStrategy : IChargingStrategy
{
    public double Calculate(double amount, bool isDefaulter)
    {
        double rate = amount switch
        {
            <= 100 => 0.00,
            <= 500 => 0.05,
            _ => 0.08
        };

        double charged = amount * (1 + rate);
        return isDefaulter ? charged * 1.03 : charged;
    }
}
```

#### PremiumChargingStrategy
```csharp
public sealed class PremiumChargingStrategy : IChargingStrategy
{
    private const double DiscountRate = 0.05;

    public double Calculate(double amount, bool isDefaulter)
    {
        return amount * (1 - DiscountRate); // desconto premium
    }
}
```

---

### 3. Cliente (BillingService)

```csharp
public class BillingService
{
    private readonly IChargingStrategy _strategy;

    /// <summary>
    /// Injeção de dependência via construtor.
    /// Recebe o CONTRATO, não a implementação concreta.
    /// </summary>
    public BillingService(IChargingStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    public double ProcessBilling(double amount, bool isDefaulter)
    {
        if (amount < 0)
            throw new ArgumentException("Valor não pode ser negativo.");

        // Delegação polimórfica via interface
        return _strategy.Calculate(amount, isDefaulter);
    }

    public string GenerateReport(double amount, bool isDefaulter)
    {
        var finalAmount = ProcessBilling(amount, isDefaulter);
        var status = isDefaulter ? "Inadimplente" : "Adimplente";
        var difference = finalAmount - amount;

        return $"Status: {status}\n" +
               $"Valor original: R$ {amount:F2}\n" +
               $"Ajuste: R$ {difference:F2}\n" +
               $"Valor final: R$ {finalAmount:F2}";
    }
}
```

**Mudança fundamental:** 
- ❌ Fase 3: Cliente conhecia `ChargingStrategyBase` (classe concreta)
- ✅ Fase 4: Cliente conhece apenas `IChargingStrategy` (contrato abstrato)

---

### 4. Composição Centralizada (Factory)

```csharp
public static class ChargingStrategyFactory
{
    private static readonly Dictionary<string, Func<IChargingStrategy>> _catalog = new()
    {
        ["standard"] = () => new StandardChargingStrategy(),
        ["penalty"] = () => new DefaulterPenaltyChargingStrategy(),
        ["progressive"] = () => new ProgressiveChargingStrategy(),
        ["premium"] = () => new PremiumChargingStrategy()
    };

    public static IChargingStrategy Resolve(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return new StandardChargingStrategy(); // fallback

        var normalizedMode = mode.ToLowerInvariant();

        if (_catalog.TryGetValue(normalizedMode, out var factory))
            return factory();

        throw new ArgumentException($"Política '{mode}' não reconhecida.", nameof(mode));
    }

    public static IEnumerable<string> GetAvailablePolicies()
    {
        return _catalog.Keys;
    }
}
```

**Benefícios:**
- ✅ Ponto ÚNICO que conhece as concretas
- ✅ Cliente não precisa de `switch` ou `new`
- ✅ Adicionar estratégia: atualizar Factory, cliente inalterado

---

### 5. Uso do Cliente (Program)

```csharp
private static void ProcessAndDisplay(double amount, bool isDefaulter, string mode)
{
    // 1. Factory resolve a política → implementação
    IChargingStrategy strategy = ChargingStrategyFactory.Resolve(mode);

    // 2. Injeção de dependência: cliente recebe o contrato
    var billingService = new BillingService(strategy);

    // 3. Cliente processa sem conhecer a concreta
    Console.WriteLine($"Política: {mode}");
    Console.WriteLine(billingService.GenerateReport(amount, isDefaulter));
}
```

**Fluxo:**
```
mode (string) → Factory.Resolve() → IChargingStrategy → BillingService → resultado
```

---

### 6. Testes com Dublês

#### Dublê: FakeChargingStrategy
```csharp
public sealed class FakeChargingStrategy : IChargingStrategy
{
    private readonly double _fixedResult;

    public FakeChargingStrategy(double fixedResult)
    {
        _fixedResult = fixedResult;
    }

    public double Calculate(double amount, bool isDefaulter)
    {
        return _fixedResult; // retorno controlado
    }
}
```

#### Teste: Usando o Fake
```csharp
private static void Test_ProcessBilling_WithFake_ReturnsExpectedValue()
{
    // Arrange: Fake sempre retorna 123.45
    var fakeStrategy = new FakeChargingStrategy(123.45);
    var service = new BillingService(fakeStrategy);

    // Act: Processa qualquer valor
    var result = service.ProcessBilling(100, false);

    // Assert: Deve retornar o valor fixo do fake
    if (Math.Abs(result - 123.45) > 0.001)
        throw new Exception($"Esperado 123.45, obtido {result}");

    Console.WriteLine("✓ Teste passou!");
}
```

**Por que usar dublês?**
- ✅ **Rápido:** Sem I/O, sem dependências externas
- ✅ **Determinístico:** Resultado previsível e controlado
- ✅ **Focado:** Testa apenas a lógica do cliente
- ✅ **Isolado:** Não depende da implementação real da estratégia

#### Dublê: SpyChargingStrategy
```csharp
public sealed class SpyChargingStrategy : IChargingStrategy
{
    public double LastAmount { get; private set; }
    public bool LastIsDefaulter { get; private set; }
    public int CallCount { get; private set; }

    public double Calculate(double amount, bool isDefaulter)
    {
        LastAmount = amount;
        LastIsDefaulter = isDefaulter;
        CallCount++;
        return amount;
    }
}
```

**Uso:** Verifica que o cliente chamou a estratégia com parâmetros corretos.

---

## Comparação com Fases Anteriores

### Fase 1 (Procedural)
```csharp
// ❌ Lógica toda em um lugar
double Calculate(double amount, bool isDefaulter, string mode)
{
    if (mode == "standard") return amount;
    else if (mode == "penalty") return amount * (isDefaulter ? 1.10 : 1.0);
    // ... mais ifs/switches
}
```

### Fase 3 (Herança)
```csharp
// ⚠️ Cliente conhece a base abstrata
ChargingStrategyBase strategy = mode switch
{
    "standard" => new StandardChargingStrategy(),
    // ... switch no cliente
};
return strategy.Charge(amount, isDefaulter);
```

### Fase 4 (Interface + Factory)
```csharp
// ✅ Cliente depende apenas do contrato
IChargingStrategy strategy = Factory.Resolve(mode); // Factory decide
var service = new BillingService(strategy);          // DI
return service.ProcessBilling(amount, isDefaulter);  // Sem conhecer concreta
```

---

## Responsabilidades

| Componente | Responsabilidade |
|------------|------------------|
| **IChargingStrategy** | Define o contrato (comportamento esperado) |
| **StandardChargingStrategy** | Implementa cobrança padrão |
| **DefaulterPenaltyChargingStrategy** | Implementa penalidade para inadimplentes |
| **ProgressiveChargingStrategy** | Implementa taxa progressiva |
| **PremiumChargingStrategy** | Implementa desconto premium |
| **ChargingStrategyFactory** | Resolve política → implementação (composição única) |
| **BillingService** | Processa cobrança usando a estratégia injetada |
| **FakeChargingStrategy** | Dublê para testes (retorno controlado) |
| **SpyChargingStrategy** | Dublê para testes (registra chamadas) |

---

## O Que Melhorou ✅

### 1. Contrato Explícito
- Interface define comportamento esperado claramente.
- Cliente sabe O QUÊ esperar, não precisa saber o COMO.
- Facilita entendimento e documentação.

### 2. Inversão de Dependência (SOLID "D")
- Cliente depende de abstração (`IChargingStrategy`), não de concretos.
- Implementações concretas também dependem da interface.
- Fluxo de dependência: **Cliente → Interface ← Concretas**.

### 3. Composição Centralizada
- Factory é o ÚNICO ponto que conhece concretos.
- Adicionar nova estratégia: atualizar Factory, cliente não muda.
- Sem `new` espalhado, sem `switch` no cliente.

### 4. Testabilidade Máxima
- Dublês (fake/stub) implementam a mesma interface.
- Testes rápidos (sem I/O), determinísticos (retorno controlado).
- Foco na lógica do cliente, não na implementação da estratégia.

### 5. Extensibilidade
- Qualquer classe pode implementar `IChargingStrategy`.
- Não requer herança de classe base.
- Facilita adicionar estratégias de terceiros.

---

## O Que Ainda Pode Melhorar 🔄

### 1. Factory Manual
- Factory ainda é manual (dicionário hardcoded).
- **Próximo passo:** Usar DI Container (Fase 6) para registro automático.

### 2. Testes Manuais
- Testes escritos manualmente sem framework.
- **Próximo passo:** Usar xUnit/NUnit para testes automatizados.

### 3. Configuração Hardcoded
- Políticas definidas no código.
- **Próximo passo:** Carregar de arquivo de configuração (JSON/XML).

---

## Exemplo de Saída

```
=== Fase 4: Interface Plugável e Testável ===

Políticas disponíveis:
  - standard
  - penalty
  - progressive
  - premium

Política: standard
Status: Adimplente
Valor original: R$ 50,00
Ajuste: R$ 0,00
Valor final: R$ 50,00
--------------------------------------------------
Política: penalty
Status: Inadimplente
Valor original: R$ 150,00
Ajuste: R$ 15,00
Valor final: R$ 165,00
--------------------------------------------------

=== Executando Testes com Dublês ===

✓ Test_ProcessBilling_WithFake_ReturnsExpectedValue
✓ Test_ProcessBilling_WithNegativeAmount_ThrowsException
✓ Test_ProcessBilling_WithSpy_CallsStrategyOnce
✓ Test_GenerateReport_FormatsCorrectly

✅ Todos os testes passaram!
```

---

## Critérios de Avaliação (Rubrica)

| Critério | Pontos | Descrição |
|----------|--------|-----------|
| Interface explícita | 0–2 | Contrato claro com assinatura enxuta |
| 2+ implementações concretas | 0–2 | Classes `sealed` implementando a interface |
| Cliente depende do contrato | 0–2 | Recebe interface, não concretos |
| Composição centralizada | 0–2 | Factory/catálogo resolve política → implementação |
| Teste com dublê | 0–2 | Fake/stub sem I/O, determinístico |
| **Total** | **0–10** | |

---

## Como Executar

### Pré-requisitos
- .NET 10.0 ou superior

### Comandos

```powershell
# Navegar até a pasta
cd c:\Projects\Interfaces\src\fase-04-com-interfaces

# Compilar
dotnet build

# Executar (inclui testes)
dotnet run
```

---

## Ligação com o Módulo

Esta fase é o **marco da alternância verdadeira**:

1. ✅ **Contrato explícito** - Interface define comportamento
2. ✅ **Composição centralizada** - Factory decide implementação
3. ✅ **Testes com dublês** - Sem I/O, rápidos e focados

Prepara para:
- **Fase 5:** SOLID Principles aprofundados
- **Fase 6:** Dependency Injection Container
- **Fase 7:** Clean Architecture e separação de camadas

---

## Referências

- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
- [Test Doubles](https://martinfowler.com/bliki/TestDouble.html)
- [Factory Pattern](https://refactoring.guru/design-patterns/factory-method)
