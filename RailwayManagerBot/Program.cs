using TelegramTool;
using Tools;

namespace RailwayManagerBot;

internal static class Program
{
    private static void Main()
    {
        TelegramManager telegramManager = new TelegramManager();
        telegramManager.StartBot();
    }
}