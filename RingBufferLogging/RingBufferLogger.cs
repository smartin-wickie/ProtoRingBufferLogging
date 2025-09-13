using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace RingBufferLogging
{
    /// <summary>
    /// A thread-safe ring buffer logger that stores log messages in a key-value store.
    /// </summary>
    public class RingBufferLogger : ILogger
    {
        private readonly string _keyPrefix;
        private readonly int _bufferSize;
        private readonly IKeyValueStore _store;
        private readonly object _lock = new object();
        private readonly string _positionKey;
        private readonly ILogger? _fallbackLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBufferLogger"/> class.
        /// </summary>
        /// <param name="store">The key-value store instance.</param>
        /// <param name="keyPrefix">The key prefix for this logger's entries.</param>
        /// <param name="bufferSize">The size of the ring buffer.</param>
        /// <param name="fallbackLogger">Optional fallback logger for error handling.</param>
        public RingBufferLogger(IKeyValueStore store, string keyPrefix, int bufferSize, ILogger? fallbackLogger = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _keyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
            _bufferSize = bufferSize > 0 ? bufferSize : throw new ArgumentOutOfRangeException(nameof(bufferSize));
            _positionKey = $"{_keyPrefix}:position";
            _fallbackLogger = fallbackLogger;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                var logEntry = new LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    LogLevel = logLevel,
                    EventId = eventId.Id,
                    Message = message,
                    Exception = exception?.ToString()
                };
                WriteLogEntry(logEntry);
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to log to ring buffer");
                Console.Error.WriteLine($"RingBufferLogger error: {ex}");
            }
        }

        private void WriteLogEntry(LogEntry entry)
        {
            lock (_lock)
            {
                int position = (int)(_store.StringIncrement(_positionKey) % _bufferSize);
                string entryKey = $"{_keyPrefix}:entry:{position}";
                _store.StringSet(entryKey, entry.ToJson());
            }
        }

        /// <summary>
        /// Retrieves the current log messages in the order they were logged.
        /// </summary>
        public List<LogEntry> GetLogEntries()
        {
            var entries = new List<LogEntry>();
            try
            {
                long position = 0;
                var posValue = _store.StringGet(_positionKey);
                if (!string.IsNullOrEmpty(posValue) && long.TryParse(posValue, out var parsed))
                    position = parsed;
                for (int i = 0; i < _bufferSize; i++)
                {
                    int idx = (int)((position + 1 + i) % _bufferSize);
                    string entryKey = $"{_keyPrefix}:entry:{idx}";
                    var value = _store.StringGet(entryKey);
                    if (!string.IsNullOrEmpty(value))
                    {
                        entries.Add(LogEntry.FromJson(value));
                    }
                }
            }
            catch (Exception ex)
            {
                _fallbackLogger?.LogError(ex, "Failed to retrieve log entries");
                Console.Error.WriteLine($"RingBufferLogger error: {ex}");
            }
            return entries;
        }

        /// <summary>
        /// Clears the ring buffer.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < _bufferSize; i++)
                {
                    string entryKey = $"{_keyPrefix}:entry:{i}";
                    _store.KeyDelete(entryKey);
                }
                _store.StringSet(_positionKey, "-1");
            }
        }

        /// <summary>
        /// Gets the size of the ring buffer.
        /// </summary>
        public int GetBufferSize() => _bufferSize;

        /// <summary>
        /// Gets the number of log messages currently stored.
        /// </summary>
        public int GetLogCount()
        {
            int count = 0;
            for (int i = 0; i < _bufferSize; i++)
            {
                string entryKey = $"{_keyPrefix}:entry:{i}";
                if (_store.KeyExists(entryKey))
                    count++;
            }
            return count;
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Represents a log entry for the ring buffer logger.
    /// </summary>
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel LogLevel { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static LogEntry FromJson(string json) => JsonSerializer.Deserialize<LogEntry>(json)!;
    }
}
