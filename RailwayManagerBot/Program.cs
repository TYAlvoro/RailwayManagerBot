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
        FileTool fileTool = new FileTool();
        fileTool.CreateDirectories();
        fileTool.CreateFile();
         TelegramManager telegramManager = new TelegramManager();
         telegramManager.StartBot();
    }
}