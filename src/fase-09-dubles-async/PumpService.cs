using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fase9_DublesAsync
{
    /// <summary>
    /// Serviço que lê itens de um reader assíncrono e escreve em um writer, com retentativa/backoff.
    /// </summary>
    public sealed class PumpService<T>
    {
        private readonly IAsyncReader<T> _reader;
        private readonly IAsyncWriter<T> _writer;
        private readonly IClock _clock;
        private readonly IAsyncDelay _delay;
        private readonly IBackoffPolicy _backoff;
        private readonly int _maxRetries;

        public PumpService(
            IAsyncReader<T> reader,
            IAsyncWriter<T> writer,
            IClock clock,
            IAsyncDelay delay,
            IBackoffPolicy backoff,
            int maxRetries = 3)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _delay = delay ?? throw new ArgumentNullException(nameof(delay));
            _backoff = backoff ?? throw new ArgumentNullException(nameof(backoff));
            _maxRetries = maxRetries;
        }

        public async Task<int> RunAsync(CancellationToken ct = default)
        {
            var count = 0;

            await foreach (var item in _reader.ReadAsync(ct).WithCancellation(ct))
            {
                var attempt = 0;
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        await _writer.WriteAsync(item, ct);
                        count++;
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception) when (++attempt <= _maxRetries)
                    {
                        // Simula backoff controlado por política/clock/delay (sem Thread.Sleep real)
                        var delay = _backoff.GetDelay(attempt);
                        _ = _clock.Now; // leitura avança clock fake nos testes
                        if (delay > TimeSpan.Zero)
                        {
                            await _delay.DelayAsync(delay, ct);
                        }
                    }
                }
            }

            return count;
        }
    }
}
