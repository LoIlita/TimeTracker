using Microsoft.Extensions.Logging;

namespace TimeTracker.UI.Services;

public class LoggerProvider
{
    private static ILoggerFactory? _loggerFactory;

    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public static ILogger<T> CreateLogger<T>()
    {
        if (_loggerFactory == null)
        {
            throw new InvalidOperationException("LoggerProvider nie został zainicjalizowany");
        }
        return _loggerFactory.CreateLogger<T>();
    }

    public static ILoggerFactory GetLoggerFactory()
    {
        if (_loggerFactory == null)
        {
            throw new InvalidOperationException("LoggerProvider nie został zainicjalizowany");
        }
        return _loggerFactory;
    }
} 