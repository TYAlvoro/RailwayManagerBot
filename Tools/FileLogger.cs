using Microsoft.Extensions.Logging;

namespace Tools;

public class FileLogger : ILogger
{
    private readonly string _filePath;
    private readonly object _lockObj = new ();

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        lock (_lockObj)
        {
            try
            {
                using (var writer = new StreamWriter(_filePath, true))
                {
                    writer.WriteLine($"{DateTime.Now} [{logLevel}] - {formatter(state, exception)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в лог: {ex.Message}");
            }
        }
    }
}