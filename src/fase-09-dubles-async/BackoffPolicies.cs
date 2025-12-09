using System;

namespace Fase9_DublesAsync
{
    public sealed class ExponentialBackoffPolicy : IBackoffPolicy
    {
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan _maxDelay;

        public ExponentialBackoffPolicy(TimeSpan? baseDelay = null, TimeSpan? maxDelay = null)
        {
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(50);
            _maxDelay = maxDelay ?? TimeSpan.FromSeconds(2);
        }

        public TimeSpan GetDelay(int attempt)
        {
            if (attempt <= 0) return TimeSpan.Zero;
            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            return delay > _maxDelay ? _maxDelay : delay;
        }
    }

    public sealed class LinearBackoffPolicy : IBackoffPolicy
    {
        private readonly TimeSpan _step;
        private readonly TimeSpan _maxDelay;

        public LinearBackoffPolicy(TimeSpan? step = null, TimeSpan? maxDelay = null)
        {
            _step = step ?? TimeSpan.FromMilliseconds(50);
            _maxDelay = maxDelay ?? TimeSpan.FromSeconds(1);
        }

        public TimeSpan GetDelay(int attempt)
        {
            if (attempt <= 0) return TimeSpan.Zero;
            var delay = TimeSpan.FromMilliseconds(_step.TotalMilliseconds * attempt);
            return delay > _maxDelay ? _maxDelay : delay;
        }
    }
}
