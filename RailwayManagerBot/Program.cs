using Microsoft.Extensions.Logging;
using TelegramTool;
using Tools;

namespace RailwayManagerBot;

internal static class Program
{
    private static void Main()
    {
        var separator = Path.DirectorySeparatorChar;
        var logPath = $"..{separator}..{separator}..{separator}..{separator}var{separator}log.txt";
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(logPath));
        });
        
        var logger = loggerFactory.CreateLogger("FileLogger");
        var fileTool = new FileTool();
        fileTool.CreateDirectories();
        fileTool.CreateStateFile();
        var telegramManager = new TelegramManager(logger);
        telegramManager.StartBot();
    }
}

