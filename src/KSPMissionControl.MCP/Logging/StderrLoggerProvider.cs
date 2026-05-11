using Microsoft.Extensions.Logging;

namespace KSPMissionControl.MCP.Logging;

// Writes log output to stderr so stdout remains clean for MCP stdio protocol traffic.
internal sealed class StderrLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new StderrLogger(categoryName);
    public void Dispose() { }
}

internal sealed class StderrLogger(string categoryName) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        Console.Error.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");
        if (exception is not null)
            Console.Error.WriteLine(exception);
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
