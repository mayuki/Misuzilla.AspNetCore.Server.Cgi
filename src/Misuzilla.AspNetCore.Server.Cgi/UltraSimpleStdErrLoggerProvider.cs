using Microsoft.Extensions.Logging;

namespace Misuzilla.AspNetCore.Server.Cgi;

internal class UltraSimpleStdErrLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
        => new Logger(categoryName);

    public void Dispose()
    {
    }

    class Logger(string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Console.Error.WriteLine($"[{logLevel}][{categoryName}] {formatter(state, exception)}");
                if (exception is not null)
                {
                    Console.Error.WriteLine(exception.ToString());
                }
            }
        }
    }
}