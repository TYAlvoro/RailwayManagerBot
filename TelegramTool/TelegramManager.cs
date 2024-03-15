using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tools;
using File = System.IO.File;

namespace TelegramTool;

public class TelegramManager
{
    private const string Token = "7041448431:AAFHhgFzGwI0sSTt65toZt5za30Do0aOjpo";
    private readonly TelegramBotClient _client = new(Token);
    
    public void StartBot()
    {
        InitBot();
    }
    
    private void InitBot()
    {
        using CancellationTokenSource cts = new();
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.ReadLine();
        cts.Cancel();
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is { } messageText)
        {
            long chatId = message.Chat.Id;
            await HandleUserMessage(client, chatId, cancellationToken, messageText);
        }

        else if (message.Document is { } messageDocument)
        {
            await HandleUserDocument(client, cancellationToken, messageDocument);
        }
    }
    
    private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        return Task.CompletedTask;
    }

    private async Task HandleUserMessage(ITelegramBotClient client, long chatId, CancellationToken cancellationToken,
        string messageText)
    {
        Typewriter typewriter = new Typewriter();
        
        if (messageText == "/start")
        {
            await client.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromFileId(
                    "CAACAgIAAxkBAAELse9l8zHKpeU113KvTTNOj0t2XEfk_QACnxIAAgKvmUsyx0PpsZAfRDQE"),
                cancellationToken: cancellationToken);
        }
        else
        {
            await typewriter.TypeMessageBySymbols(client, chatId, cancellationToken, $"You said: {messageText}");
        }
    }

    private async Task HandleUserDocument(ITelegramBotClient client, CancellationToken cancellationToken, 
        Document document)
    {
        var fileInfo = await client.GetFileAsync(document.FileId, cancellationToken);
        var filePath = fileInfo.FilePath;
        var destinationFilePath = $"D:/Desktop/{document.FileName}";
        
        await using var fileStream = File.Create(destinationFilePath);
        await client.DownloadFileAsync(
            filePath: filePath!,
            destination: fileStream,
            cancellationToken: cancellationToken);
    }
}