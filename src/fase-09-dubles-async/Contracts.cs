using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fase9_DublesAsync
{
    // Contratos mínimos para costuras externas.
    public interface IClock { DateTimeOffset Now { get; } }
    public interface IIdGenerator { string NewId(); }
    public interface IAsyncReader<T> { IAsyncEnumerable<T> ReadAsync(CancellationToken ct = default); }
    public interface IAsyncWriter<T> { Task WriteAsync(T item, CancellationToken ct = default); }

    // Auxiliar para evitar Thread.Sleep real em retentativas.
    public interface IAsyncDelay { Task DelayAsync(TimeSpan delay, CancellationToken ct = default); }

    // Política de backoff parametrizável.
    public interface IBackoffPolicy { TimeSpan GetDelay(int attempt); }
}
