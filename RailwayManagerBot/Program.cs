using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTool;

namespace RailwayManagerBot;

internal static class Program
{
    private static void Main()
    {
        TelegramManager telegramManager = new TelegramManager();
        telegramManager.StartBot();
    }
}