using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// A simple in-memory logger that captures log messages to a list
    /// Useful for tests that need to inspect logged output
    /// </summary>
    public class InMemoryLogger : ILogger
    {
        private readonly List<string> _logMessages = new List<string>();
        private readonly LogLevel _minLevel;

        public InMemoryLogger(LogLevel minLevel = LogLevel.Information)
        {
            _minLevel = minLevel;
        }

        public IReadOnlyList<string> LogMessages => _logMessages.AsReadOnly();

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            _logMessages.Add($"[{logLevel}] {message}");
        }

        public void Clear() => _logMessages.Clear();
    }

    /// <summary>
    /// Logger factory for creating InMemoryLogger instances
    /// </summary>
    public class InMemoryLogger<T> : InMemoryLogger, ILogger<T>
    {
        public InMemoryLogger(LogLevel minLevel = LogLevel.Information) : base(minLevel)
        {
        }
    }
}
