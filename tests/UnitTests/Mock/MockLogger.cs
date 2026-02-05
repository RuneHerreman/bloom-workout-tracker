using Microsoft.Extensions.Logging;

namespace UnitTests.Mocks;

public class MockLogger<T> : ILogger<T>
{
    public List<string> LoggedMessages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LoggedMessages.Add(formatter(state, exception));
    }

    public void Clear()
    {
        LoggedMessages.Clear();
    }
}