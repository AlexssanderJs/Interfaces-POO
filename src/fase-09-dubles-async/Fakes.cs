using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fase9_DublesAsync
{
    public sealed class FakeClock : IClock
    {
        private DateTimeOffset _now;
        private readonly TimeSpan _tick;

        public FakeClock(DateTimeOffset? start = null, TimeSpan? tick = null)
        {
            _now = start ?? DateTimeOffset.UnixEpoch;
            _tick = tick ?? TimeSpan.FromMilliseconds(10);
        }

        public DateTimeOffset Now
        {
            get
            {
                var current = _now;
                _now = _now.Add(_tick); // avança a cada leitura para medir backoff sem dormir
                return current;
            }
        }

        public void Advance(TimeSpan delta) => _now = _now.Add(delta);
    }

    public sealed class FakeDelay : IAsyncDelay
    {
        private readonly FakeClock _clock;

        public FakeDelay(FakeClock clock)
        {
            _clock = clock;
        }

        public Task DelayAsync(TimeSpan delay, CancellationToken ct = default)
        {
            _clock.Advance(delay);
            return Task.CompletedTask;
        }
    }

    public sealed class FakeIdGenerator : IIdGenerator
    {
        private int _counter;
        public string NewId() => $"id-{Interlocked.Increment(ref _counter)}";
    }

    public sealed class FakeReader<T> : IAsyncReader<T>
    {
        private readonly List<T> _items;
        private readonly int? _failAtIndex;

        public FakeReader(IEnumerable<T> items, int? failAtIndex = null)
        {
            _items = items.ToList();
            _failAtIndex = failAtIndex;
        }

        public async IAsyncEnumerable<T> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                if (_failAtIndex.HasValue && i == _failAtIndex.Value)
                    throw new InvalidOperationException("Reader failure");

                yield return _items[i];
                await Task.Yield();
            }
        }
    }

    public sealed class FakeWriter<T> : IAsyncWriter<T>
    {
        private readonly List<T> _written = new();
        private int _remainingFailures;
        private readonly Action? _onWrite;
        private readonly bool _invokeAfterSuccess;

        public FakeWriter(int failCount = 0, Action? onWrite = null, bool invokeAfterSuccess = false)
        {
            _remainingFailures = failCount;
            _onWrite = onWrite;
            _invokeAfterSuccess = invokeAfterSuccess;
        }

        public IReadOnlyList<T> Written => _written;
        public int Attempts { get; private set; }

        public async Task WriteAsync(T item, CancellationToken ct = default)
        {
            Attempts++;

            if (!_invokeAfterSuccess)
                _onWrite?.Invoke();

            ct.ThrowIfCancellationRequested();

            if (_remainingFailures > 0)
            {
                _remainingFailures--;
                throw new InvalidOperationException("Writer failure");
            }

            await Task.Yield();
            _written.Add(item);

            if (_invokeAfterSuccess)
                _onWrite?.Invoke();
        }
    }
}
