## Fase 9 — Dublês avançados e testes assíncronos (async, streams e tempo controlado)

### Objetivo
Extrair dependências externas para contratos mínimos (tempo, geração de IDs, leitura/escrita assíncrona) e testar com dublês previsíveis (fakes/stubs), cobrindo sucesso, erro, cancelamento e streams (`IAsyncEnumerable<T>`). Demonstrar retentativa/backoff sem `Thread.Sleep`, usando clock/delay fakes.

---

## Arquitetura

```
┌────────────────────────────────────────────────┐
│                 Cliente (Program)              │
└───────────────┬────────────────────────────────┘
                │ depende de
                ▼
┌────────────────────────────────────────────────┐
│               PumpService<T>                   │
│  lê do reader → escreve no writer              │
│  retentativa + backoff (policy + delay fake)   │
└───────────────┬────────────────────────────────┘
                │ usa
                ▼
┌────────────────────────────────────────────────┐
│  Contratos mínimos                             │
│  - IClock (Now)                                │
│  - IIdGenerator (NewId)                        │
│  - IAsyncReader<T> (stream)                    │
│  - IAsyncWriter<T> (sink)                      │
│  - IAsyncDelay (delay fake)                    │
│  - IBackoffPolicy (política)                   │
└────────────────────────────────────────────────┘
```

---

## Contratos

```csharp
public interface IClock { DateTimeOffset Now { get; } }
public interface IIdGenerator { string NewId(); }
public interface IAsyncReader<T> { IAsyncEnumerable<T> ReadAsync(CancellationToken ct = default); }
public interface IAsyncWriter<T> { Task WriteAsync(T item, CancellationToken ct = default); }
public interface IAsyncDelay { Task DelayAsync(TimeSpan delay, CancellationToken ct = default); }
public interface IBackoffPolicy { TimeSpan GetDelay(int attempt); }
```

---

## Serviço (PumpService)

```csharp
public sealed class PumpService<T>
{
    public async Task<int> RunAsync(CancellationToken ct = default)
    {
        var count = 0;
        await foreach (var item in _reader.ReadAsync(ct).WithCancellation(ct))
        {
            var attempt = 0;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                try { await _writer.WriteAsync(item, ct); count++; break; }
                catch when (++attempt <= _maxRetries)
                {
                    var delay = _backoff.GetDelay(attempt);
                    _ = _clock.Now; // avança clock fake
                    if (delay > TimeSpan.Zero)
                        await _delay.DelayAsync(delay, ct);
                }
            }
        }
        return count;
    }
}
```

---

## Dublês

```csharp
// Clock que avança a cada leitura
public sealed class FakeClock : IClock { /* Now avança; Advance(delta) */ }

// Delay fake: avança o clock, não dorme
public sealed class FakeDelay : IAsyncDelay { /* Advance no clock */ }

// Reader fake: emite itens via IAsyncEnumerable<T>, opcionalmente falha no índice
public sealed class FakeReader<T> : IAsyncReader<T> { /* ReadAsync yield */ }

// Writer fake: pode falhar N vezes, captura itens escritos, suporta callback (cancelar)
public sealed class FakeWriter<T> : IAsyncWriter<T> { /* WriteAsync */ }

// Backoff: exponencial ou linear
public sealed class ExponentialBackoffPolicy : IBackoffPolicy { /* baseDelay, maxDelay */ }
public sealed class LinearBackoffPolicy : IBackoffPolicy { /* step, maxDelay */ }
```

---

## Testes (roteiro atendido)

1) **Sucesso simples:** 3 itens, writer sempre escreve → retorna 3.  
2) **Retentativa:** writer falha 2x e depois escreve → retorna 3; attempts > 3; clock avançado via delay fake.  
3) **Cancelamento:** cancela após 1 item → `OperationCanceledException`, contagem parcial.  
4) **Stream vazio:** 0 itens → retorna 0.  
5) **Erro no meio:** reader lança no 2º item → exceção propagada, apenas 1 item escrito.

Execução em `Program.Main` roda a demo + todos os testes.

---

## Como executar

```powershell
cd c:\Projects\Interfaces\src\fase-08-dubles-async
dotnet build
dotnet run
```

---

## Decisões
- Sem `Thread.Sleep`: usamos `IAsyncDelay` fake para avançar clock (determinístico).
- Política configurável: `IBackoffPolicy` (exponencial/linear) para retentativa.
- Costuras claras: `IClock`, `IIdGenerator`, `IAsyncReader`, `IAsyncWriter` desacoplam I/O real.
- Testes assíncronos com `IAsyncEnumerable<T>` e `CancellationToken` cobrindo sucesso, erro, cancelamento.

---

## Limitações
- Sem concorrência/parallelismo (processa 1 item por vez).
- Sem logging/telemetria (simplicidade didática).
- Sem métricas de jitter/backoff distribuído.
- Sem política de circuito aberto.

---

## Próximos passos
- Introduzir circuito aberto (circuit breaker).
- Incluir jitter no backoff e métricas.
- Adicionar execução paralela/particionada do stream.
