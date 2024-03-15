using Tools;

namespace RailwayManagerBot;

internal static class Program
{
    private static void Main()
    {
        // TelegramManager telegramManager = new TelegramManager();
        // telegramManager.StartBot();
        FileTool fileTool = new FileTool();
        fileTool.CreateDirectories();
    }
}