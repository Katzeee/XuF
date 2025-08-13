using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Xuf.Core
{
    /// <summary>
    /// Asynchronous wrapper for file logging using a producer-consumer queue.
    /// At-most-once delivery with bounded capacity. Dropped items are counted.
    /// </summary>
    internal sealed class AsyncFileAppender : ILogAppender
    {
        private readonly FileAppender _inner;
        private readonly ConcurrentQueue<LogEvent> _queue = new ConcurrentQueue<LogEvent>();
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly Thread _worker;

        private readonly int _capacity;
        private readonly int _batchSize;
        private readonly int _flushIntervalMs;

        private int _queuedCount;
        private int _droppedCount;
        private volatile bool _running;

        /// <summary>
        /// Number of log events dropped due to queue overflow.
        /// </summary>
        public int DroppedCount => _droppedCount;

        public AsyncFileAppender(string filePath, int capacity = 8192, int batchSize = 128, int flushIntervalMs = 200, ILogFormatter formatter = null)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            if (flushIntervalMs < 1) throw new ArgumentOutOfRangeException(nameof(flushIntervalMs));

            var dir = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

            _inner = new FileAppender(filePath, formatter);
            _capacity = capacity;
            _batchSize = batchSize;
            _flushIntervalMs = flushIntervalMs;

            _running = true;
            _worker = new Thread(ConsumeLoop) { IsBackground = true, Name = "AsyncFileAppender" };
            _worker.Start();
        }

        public void Append(in LogEvent logEvent)
        {
            // Bounded queue; drop newest on overflow
            if (Interlocked.Increment(ref _queuedCount) > _capacity)
            {
                Interlocked.Decrement(ref _queuedCount);
                Interlocked.Increment(ref _droppedCount);
                return;
            }

            _queue.Enqueue(logEvent);
            _signal.Set();
        }

        public void Flush()
        {
            // Best effort: wake consumer and flush inner
            _signal.Set();
            _inner.Flush();
        }

        public void Dispose()
        {
            try
            {
                _running = false;
                _signal.Set();
                // Attempt to drain remaining events with a time budget
                var sw = Stopwatch.StartNew();
                const int drainBudgetMs = 200;
                while (_queuedCount > 0 && sw.ElapsedMilliseconds < drainBudgetMs)
                {
                    Thread.Sleep(10);
                }

                _inner.Flush();
            }
            catch { /* ignore */ }
            finally
            {
                try { _worker.Join(250); } catch { /* ignore */ }
                _signal.Dispose();
                _inner.Dispose();
            }
        }

        private void ConsumeLoop()
        {
            int processedSinceFlush = 0;
            while (_running)
            {
                // Wait for signal or periodic flush
                _signal.WaitOne(_flushIntervalMs);

                // Drain up to batch size
                int processed = 0;
                while (processed < _batchSize)
                {
                    if (!_queue.TryDequeue(out var evt))
                        break;

                    Interlocked.Decrement(ref _queuedCount);
                    _inner.Append(evt);
                    processed++;
                    processedSinceFlush++;
                }

                if (processedSinceFlush > 0)
                {
                    _inner.Flush();
                    processedSinceFlush = 0;
                }
            }

            // Final drain when stopping
            while (_queue.TryDequeue(out var evt))
            {
                Interlocked.Decrement(ref _queuedCount);
                _inner.Append(evt);
            }
            _inner.Flush();
        }
    }
}


