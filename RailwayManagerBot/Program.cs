using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using InnerProcesses;
using TelegramTool;
using Tools;

namespace RailwayManagerBot;

internal static class Program
{
    private static void Main()
    {
        var fileTool = new FileTool();
        fileTool.CreateDirectories();
        fileTool.CreateFile();
        var telegramManager = new TelegramManager();
        telegramManager.StartBot();
    }
}