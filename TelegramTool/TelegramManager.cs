﻿using InnerProcesses;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tools;
using File = System.IO.File;

namespace TelegramTool;

public class TelegramManager
{
    private readonly TelegramBotClient _client;
    private const string Token = "7041448431:AAFHhgFzGwI0sSTt65toZt5za30Do0aOjpo";

    public TelegramManager()
    {
        _client = new TelegramBotClient(Token);
    }

    public void StartBot()
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
        var state = new State();
        
        if (update.Message is { } message)
        {
            var chatId = message.Chat.Id;
            state.AddUser(chatId);
            
            if (message.Text is { } messageText)
            {
                await HandleUserMessage(client, chatId, cancellationToken, messageText, state);
            }
            else if (message.Document is { } messageDocument)
            {
                await HandleUserDocument(client, chatId, cancellationToken, messageDocument);
            }
        }
        else if (update.CallbackQuery is { } callbackQuery)
        {
            var chatId = callbackQuery.Message!.Chat.Id;

            switch (callbackQuery.Data)
            {
                case "sortyear":
                {
                    var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                    await HandleCallbackSortYear(client, chatId, cancellationToken, stations, state);
                    break;
                }
                case "sortname":
                {
                    var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                    await HandleCallbackSortName(client, chatId, cancellationToken, stations, state);
                    break;
                }
                case "filtername":
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    state.AddStateToUser(chatId, "name");
                    break;
                case "filterline":
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    state.AddStateToUser(chatId, "line");
                    break;
                case "filterboth":
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    state.AddStateToUser(chatId, "two");
                    break;
            }
        }
    }
    
    // TODO: человеческий вывод ошибок
    private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        return Task.CompletedTask;
    }

    private async Task HandleUserMessage(ITelegramBotClient client, long chatId, CancellationToken cancellationToken,
        string messageText, State state)
    {
        switch (messageText)
        {
            case "/start":
                await HandleCommandStart(client, chatId, cancellationToken);
                break;
            case "/help":
                await HandleCommandHelp(client, chatId, cancellationToken);
                break;
            case "/work":
                await HandleCommandWork(client, chatId, cancellationToken);
                break;
            
            default:
            {
                var userState = state.UserState(chatId);
                if (new[] { "name", "line", "two" }.All(str => str != userState))
                {
                    await SayToEnterCommand(client, chatId, cancellationToken);
                    return;
                }
            
                var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                
                if (state.UserState(chatId) == "name")
                    await HandleCallbackFilterName(client, chatId, cancellationToken, stations, state, messageText);
                else if (state.UserState(chatId) == "line")
                    await HandleCallbackFilterLine(client, chatId, cancellationToken, stations, state, messageText);
                else if (state.UserState(chatId) == "two")
                { 
                    var splitText = messageText.Split(";");

                    if (splitText.Length == 2)
                    {
                        await HandleCallbackFilterBoth(client, chatId, cancellationToken, stations, 
                            state, splitText[0], splitText[1]);
                    }
                    else
                    {
                        await client.SendStickerAsync(
                            chatId: chatId,
                            sticker: InputFile.FromFileId(
                                "CAACAgIAAxkBAAELvwFl-YN7te2LzgGuZ51syGz6szWU1gACkzoAAvsUwUugxjZN527HszQE"),
                            cancellationToken: cancellationToken);
                        await client.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Требуются два значения, разделенные точкой с запятой (Выхино;Январь)!\n" +
                                  "Выбери фильтр в меню снова и введи корректные данные!",
                            cancellationToken: cancellationToken);
                    }
                }
                
                state.AddStateToUser(chatId, "false");
                break;
            }
        }
    }

    private async Task HandleUserDocument(ITelegramBotClient client, long chatId, CancellationToken cancellationToken, 
        Document document)
    {
        var fileInfo = await client.GetFileAsync(document.FileId, cancellationToken);
        var filePath = fileInfo.FilePath;
        var fileExtension = Path.GetExtension(filePath);

        if (fileExtension != ".csv" && fileExtension != ".json")
        {
            await client.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromFileId(
                    "CAACAgIAAxkBAAELu5Nl9y55vHR5iso2tQtEkchEZ_jbFgAC_ysAAu8HyUi2RnsvXMZDTzQE"),
                cancellationToken: cancellationToken);
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: "Отправлять нужно только файлы в формате json или csv!\nС другими я не знаю, что делать.",
                cancellationToken: cancellationToken);
            return;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(document.FileName);
        fileName = $"{fileName}_{chatId}{fileExtension}";
        
        var separator = Path.DirectorySeparatorChar;
        var destinationFilePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}input{separator}{fileName}";
        
        await using (var stream = File.Create(destinationFilePath))
        {
            await client.DownloadFileAsync(
                filePath: filePath!,
                destination: stream,
                cancellationToken: cancellationToken);
        }

        await SayFileDownloaded(client, chatId, cancellationToken);
        
        var state = new State();
        state.AddFileToUser(chatId, destinationFilePath);
        
        await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
    }

    private async Task<MetroStation[]> ProcessFile(string filePath, ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken)
    {
        MetroStation[] stations;
        
        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing();
            using var streamReader = new StreamReader(filePath);
            stations = await csvProcessing.Read(streamReader, client, chatId, cancellationToken);
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            using var streamReader = new StreamReader(filePath);
            stations = await jsonProcessing.Read(streamReader, client, chatId, cancellationToken);
        }

        return stations;
    }

    private async Task UploadFile(FileStream stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
        await using var streamWriter = new StreamWriter(stream);
        var state = new State();
        await client.SendDocumentAsync(
            chatId: chatId,
            document: InputFile.FromStream(stream: stream, fileName: Path.GetFileName(state.PathToFile(chatId))),
            caption: "Возвращаю обработанный файл!", cancellationToken: cancellationToken);
    }

    private async Task HandleCommandStart(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELu4dl9ydA5XUPTdejX8u7tto5BRNM9QACWQ8AAus7mUsgSBdHQAcd8jQE"),
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Привет!\nЯ помогу тебе удобно просматривать информацию о станциях метро.\n" +
                  "Просто жмыкай на /help и поехали!",
            cancellationToken: cancellationToken);
    }
    
    private async Task HandleCommandHelp(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELu4tl9yplzMzTeavndUed4OdVfcubSgACRBAAAiK3mEvG2WKIDbGYUzQE"),
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Да ладно, шучу.\nДля тебя я всегда свободен!\n\nНа самом деле здесь все просто: " +
                  "пишешь /work и выполняешь дальнейшие команды.",
            cancellationToken: cancellationToken);
    }
    
    private async Task HandleCommandWork(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELu5Fl9yzsv_SY7qB2cV4Pyqya9Zkr9QACZTAAAs7woEicWchhTvQKJDQE"),
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "It`s work time!\nТвоя задача проста - отправляешь мне файлик в формате json или csv," +
                  " выбираешь в меню, что я должен с ним сделать, а я возвращаю тебе обработанную версию.",
            cancellationToken: cancellationToken);
    }
    
    private async Task HandleCallbackSortYear(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state)
    {
        var dataTool = new DataTool();
        stations = dataTool.SortByYear(stations);
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
        }
        
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    private async Task HandleCallbackSortName(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state)
    {
        var dataTool = new DataTool();
        stations = dataTool.SortByName(stations);
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
        }
        
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    private async Task HandleCallbackFilterName(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterValue)
    {
        var dataTool = new DataTool();
        stations = dataTool.Filter(stations, filterValue, "name");
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
        }
        
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    private async Task HandleCallbackFilterLine(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterValue)
    {
        var dataTool = new DataTool();
        var filePath = state.PathToFile(chatId);

        FileStream stream;
        stations = dataTool.Filter(stations, filterValue, "line");

        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
        }
        
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    private async Task HandleCallbackFilterBoth(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterName, string filterMonth)
    {
        var dataTool = new DataTool();
        var filePath = state.PathToFile(chatId);

        FileStream stream;
        stations = dataTool.FilterByTwoFields(stations, filterName, filterMonth);

        if (Path.GetExtension(filePath) == ".csv")
        {
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
        }
        else
        {
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
        }
        
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    private async Task SayFileDownloaded(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELu5dl9y-bmYP3Zg5lLyz-GpiBP6-fOAACERgAAv564UuysbzruCnEmzQE"),
            cancellationToken: cancellationToken);

        InlineKeyboardMarkup sortKeyboard = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "По году.", callbackData: "sortyear"),
                InlineKeyboardButton.WithCallbackData(text: "По названию.", callbackData: "sortname"),
            }
        });

        InlineKeyboardMarkup filterKeyboard = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "По названию станции.",
                    callbackData: "filtername"),
                InlineKeyboardButton.WithCallbackData(text: "По названию линии.",
                    callbackData: "filterline")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "По месяцу и названию.",
                    callbackData: "filterboth")
            }
        });
        
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Файл успешно загружен в систему!\nТеперь выбери, что я должен с ним сделать.",
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Отсортировать:",
            replyMarkup: sortKeyboard,
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Отфильтровать:",
            replyMarkup: filterKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SayToEnterCommand(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELvvBl-XBHpXk3QpslJVikPXEcREJ_-gACHRgAAq9XeEqFjdt8DbqGFjQE"),
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Упс, что-то не то!\nЗабыл, что делать?\nНапоминаю: жмыкай /help.",
            cancellationToken: cancellationToken);
    }
    
    private async Task SayAboutRightValues(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELvwNl-YRPNsffUiJ_xT_0PSf2DLlQhAACahkAAppXkUvgH10YdvVpyjQE"),
            cancellationToken: cancellationToken);
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Теперь нужно ввести значения для фильтров.\nЕсли были выбраны верхние пункты, " +
                  "то достаточно ввести одно значение и отправить его.\nВ противном случае нужно ввести через пробел " +
                  "значения названия станции и месяца (именно в таком порядке (Выхино;Январь)!)",
            cancellationToken: cancellationToken);
    }
}