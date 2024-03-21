using Microsoft.Extensions.Logging;

namespace Tools;

/// <summary>
/// Класс для создания логгера в файл.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Создание логгера.
    /// </summary>
    /// <param name="categoryName">Категория логгера.</param>
    /// <returns>Объект ILogger с созданным логгером.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_filePath);
    }

    /// <summary>
    /// Инициализация требуемого ILoggerProvider метода.
    /// </summary>
    public void Dispose() { }
}