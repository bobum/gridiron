using Microsoft.Extensions.Logging;
using System.Text;

namespace GameManagement.Logging;

public class StringLogger : ILogger
{
    private readonly StringBuilder _stringBuilder;

    public StringLogger(StringBuilder stringBuilder)
    {
        _stringBuilder = stringBuilder;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        _stringBuilder.AppendLine(formatter(state, exception));
    }
}