# Fase 9 — Dublês avançados e testes assíncronos

## Resumo
Dublês (fakes/stubs) para clock, IDs, leitura e escrita assíncronas, com retentativa/backoff sem `Thread.Sleep`. Testes cobrem sucesso, erro, cancelamento e streams (`IAsyncEnumerable<T>`).

---

## Estrutura
```
IClock / IIdGenerator / IAsyncReader<T> / IAsyncWriter<T> / IAsyncDelay / IBackoffPolicy
    ↓
PumpService<T>  (lê do reader, escreve no writer, retentativa/backoff)
    ↓
Fakes (Clock, Delay, Reader, Writer) + Backoff (Exponential/Linear)
```

---

## Arquivos
- Contracts.cs: IClock, IIdGenerator, IAsyncReader, IAsyncWriter, IAsyncDelay, IBackoffPolicy
- PumpService.cs: Loop de leitura/escrita com retentativa/backoff
- BackoffPolicies.cs: Exponential e Linear
- Fakes.cs: Clock, Delay, IdGenerator, Reader, Writer
- PumpServiceTests.cs: 5 testes assíncronos cobrindo sucesso, retentativa, cancelamento, stream vazio, erro no meio
- Program.cs: Demo + execução dos testes

---

## Operações principais (PumpService)
- `RunAsync(ct)`: lê do reader, tenta escrever; em falha, aplica backoff (policy + delay fake), respeita `CancellationToken`, repete até `_maxRetries`.

---

## Como executar
```powershell
cd c:\Projects\Interfaces\src\fase-08-dubles-async
dotnet build
dotnet run
```

---

## Cobertura de testes
- Sucesso simples (3 itens)
- Retentativa com falha inicial do writer
- Cancelamento após 1 item (`OperationCanceledException`)
- Stream vazio
- Erro no meio do stream (reader lança)

---

## Decisões
- `IAsyncDelay` fake avança clock, evitando espera real.
- Política de backoff configurável (exponencial/linear).
- Dublês previsíveis permitem testar sem I/O real.
