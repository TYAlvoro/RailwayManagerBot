using Microsoft.Extensions.Logging;
using TelegramTool;
using Tools;

namespace RailwayManagerBot;

/// <summary>
/// Основной класс программы.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    private static void Main()
    {
        // Создание нужных для работы директорий и файла для стейтов.
        var fileTool = new FileTool();
        fileTool.CreateDirectories();
        fileTool.CreateStateFile();
        
        // Получение пути к логгеру и создание последнего.
        var separator = Path.DirectorySeparatorChar;
        var logPath = $"..{separator}..{separator}..{separator}..{separator}var{separator}log.txt";
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(logPath));
        });
        var logger = loggerFactory.CreateLogger("FileLogger");
        
        // Запуск бота.
        var telegramManager = new TelegramManager(logger);
        telegramManager.StartBot();
    }
}

