using InnerProcesses;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tools;
using File = System.IO.File;

namespace TelegramTool;

/// <summary>
/// Класс для работы с телеграм-клиентом.
/// </summary>
public class TelegramManager
{
    private readonly TelegramBotClient _client;
    private const string Token = "7041448431:AAFHhgFzGwI0sSTt65toZt5za30Do0aOjpo";
    private ILogger _logger;

    public TelegramManager(ILogger logger)
    {
        _logger = logger;
        _client = new TelegramBotClient(Token);
    }

    /// <summary>
    /// Метод, запускающий бота.
    /// </summary>
    public void StartBot()
    {
        // Создание всех нужных для работы клиента опций.
        using CancellationTokenSource cts = new();
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        _logger.LogInformation("Начало работы бота.");
        
        // Начало обработки апдейтов.
        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
            
        Console.ReadLine();
        cts.Cancel();
    }
    
    /// <summary>
    /// Обработчик всех апдейтов.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="update">Объект апдейтов.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        // Создание нового объекта стейтов.
        var state = new State();
        
        // Обработка сообщений.
        if (update.Message is { } message)
        {
            var chatId = message.Chat.Id;
            
            _logger.LogInformation($"Запись в файл стейта пользователя с id {chatId}");
            state.AddUser(chatId);
            
            // Обработка всех текстовых сообщений.
            if (message.Text is { } messageText)
            {
                _logger.LogInformation($"Получено сообщение от пользователя с id {chatId}. Текст: {messageText}");
                await HandleUserMessage(client, chatId, cancellationToken, messageText, state);
            }
            
            // Обработка документов, посланных пользователем.
            else if (message.Document is { } messageDocument)
            {
                _logger.LogInformation($"Получено документ от пользователя с id {chatId}. " +
                                       $"Файл: {messageDocument.FileName}");
                await HandleUserDocument(client, chatId, cancellationToken, messageDocument);
            }
        }
        
        // Обработка коллбэков (нажатий на кнопки).
        else if (update.CallbackQuery is { } callbackQuery)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            _logger.LogInformation($"Получен callback от пользователя с id {chatId}. Callback data: {callbackQuery.Data}");

            // Смотрим, что нажал пользователь.
            switch (callbackQuery.Data)
            {
                // Сортировка по году.
                case "sortyear":
                {
                    _logger.LogInformation($"Сортировка файла по году для пользователя с id {chatId}.");
                    
                    // Получение массива объектов станций с помощью обработки файла.
                    var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                    await HandleCallbackSortYear(client, chatId, cancellationToken, stations, state);
                    break;
                }
                
                // Сортировка по названию станции.
                case "sortname":
                {
                    _logger.LogInformation($"Сортировка файла по названию для пользователя с id {chatId}.");
                    
                    // Получение массива объектов станций с помощью обработки файла.
                    var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                    await HandleCallbackSortName(client, chatId, cancellationToken, stations, state);
                    break;
                }
                
                // Фильтрация по названию станции.
                case "filtername":
                    _logger.LogInformation($"Фильтрация файла по имени для пользователя с id {chatId}.");
                    
                    // Сообщение пользователю о корректных данных для ввода.
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    
                    // Запись в стейт о том, что фильтруем по названию.
                    state.AddStateToUser(chatId, "name");
                    break;
                
                // Фильтрация по названию линии.
                case "filterline":
                    _logger.LogInformation($"Фильтрация файла по линии для пользователя с id {chatId}.");
                    
                    // Сообщение пользователю о корректных данных для ввода.
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    
                    // Запись в стейт о том, что фильтруем по названию линии.
                    state.AddStateToUser(chatId, "line");
                    break;
                case "filterboth":
                    _logger.LogInformation($"Фильтрация файла по обоим значениям для пользователя с id {chatId}.");
                    
                    // Сообщение пользователю о корректных данных для ввода.
                    await SayAboutRightValues(client, chatId, cancellationToken);
                    
                    // Запись в стейт о том, что фильтруем по обоим значениям.
                    state.AddStateToUser(chatId, "two");
                    break;
            }
        }
    }
    
    /// <summary>
    /// Обработчик ошибок.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="exception">Ошибка.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <returns>Завершенный Task.</returns>
    private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogCritical(errorMessage);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Обработчик всех пользовательских сообщений.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="messageText">Текст сообщения от пользователя.</param>
    /// <param name="state">Объект стейта.</param>
    private async Task HandleUserMessage(ITelegramBotClient client, long chatId, CancellationToken cancellationToken,
        string messageText, State state)
    {
        // Обработчик различных команд и остальных сообщений.
        switch (messageText)
        {
            case "/start":
                _logger.LogInformation($"Получена команда /start от пользователя с id: {chatId}");
                await HandleCommandStart(client, chatId, cancellationToken);
                break;
            case "/help":
                _logger.LogInformation($"Получена команда /help от пользователя с id: {chatId}");
                await HandleCommandHelp(client, chatId, cancellationToken);
                break;
            case "/work":
                _logger.LogInformation($"Получена команда /work от пользователя с id: {chatId}");
                await HandleCommandWork(client, chatId, cancellationToken);
                break;
            case "/files":
                _logger.LogInformation($"Получена команда /files от пользователя с id: {chatId}");
                await HandleCommandFiles(client, chatId, cancellationToken);
                break;
            
            // Если получено текстовое сообщение, то проверяем в стейте, фильтрует ли что-то пользователь.
            // Если фильтрует, то выполняем фильтрацию. Иначе говорим, что нужно прислать файл.
            default:
            {
                _logger.LogInformation($"Получено сообщение с текстом {messageText} от пользователя с id: {chatId}");
                var userState = state.UserState(chatId);
                
                // Проверка на наличие данных о фильтрации в стейте.
                if (new[] { "name", "line", "two" }.All(str => str != userState))
                {
                    _logger.LogInformation($"Пришла отмена обработки на сообщение с текстом" +
                                           $" {messageText} от пользователя с id: {chatId}");
                    await SayToEnterCommand(client, chatId, cancellationToken);
                    return;
                }
            
                // Получение объектов станций из файла пользователя.
                var stations = await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
                _logger.LogInformation($"Считана информация из файла: {state.PathToFile(chatId)}");
                
                // Вызов различных сортировок.
                if (state.UserState(chatId) == "name")
                    await HandleCallbackFilterName(client, chatId, cancellationToken, stations, state, messageText);
                
                else if (state.UserState(chatId) == "line")
                    await HandleCallbackFilterLine(client, chatId, cancellationToken, stations, state, messageText);
                
                else if (state.UserState(chatId) == "two")
                { 
                    // Если пользователь сортирует два значения, то и фильтров должно быть два.
                    var splitText = messageText.Split(";");

                    if (splitText.Length == 2)
                    {
                        await HandleCallbackFilterBoth(client, chatId, cancellationToken, stations, 
                            state, splitText[0], splitText[1]);
                    }
                    else
                    {
                        _logger.LogInformation($"Пришла отмена фильтрации на сообщение с текстом" +
                                               $" {messageText} от пользователя с id: {chatId}");
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
                
                _logger.LogInformation($"Очищение стейта для пользователя с id: {chatId}");
                
                // Удаляем информацию о фильтрации из стейта: пользователь уже ничего не фильтрует.
                state.AddStateToUser(chatId, "false");
                break;
            }
        }
    }

    /// <summary>
    /// Обработчик пользовательских документов.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="document">Документ пользователя.</param>
    private async Task HandleUserDocument(ITelegramBotClient client, long chatId, CancellationToken cancellationToken, 
        Document document)
    {
        _logger.LogInformation($"Начата обработка документа для пользователя с id: {chatId}");
        
        // Получение информации об отправленном файле.
        var fileInfo = await client.GetFileAsync(document.FileId, cancellationToken);
        var filePath = fileInfo.FilePath;
        var fileExtension = Path.GetExtension(filePath);

        // Проверка, что файл подходящего формата.
        if (fileExtension != ".csv" && fileExtension != ".json")
        {
            _logger.LogInformation($"Пользователь с id: {chatId} отправил файл некорректного формата");
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
        
        // Получение дополнительной информации о файле и пути к нему в системе.
        var fileName = Path.GetFileNameWithoutExtension(document.FileName);
        fileName = $"{fileName}_{chatId}{fileExtension}";
        
        var separator = Path.DirectorySeparatorChar;
        var destinationFilePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}input{separator}{fileName}";
        
        _logger.LogInformation($"Начата загрузка документа пользователя с id: {chatId}");
        
        // Загрузка файла в систему.
        await using (var stream = File.Create(destinationFilePath))
        {
            await client.DownloadFileAsync(
                filePath: filePath!,
                destination: stream,
                cancellationToken: cancellationToken);
        }
        _logger.LogInformation($"Окончена загрузка документа пользователя с id: {chatId}");
        
        await SayFileDownloaded(client, chatId, cancellationToken);
        
        // Добавление пути к файлу в стейт пользователя.
        var state = new State();
        _logger.LogInformation($"Добавление пути до файла: {destinationFilePath} для пользователя с id: {chatId}");
        state.AddFileToUser(chatId, destinationFilePath);
        
        // Запуск обработки файла.
        _logger.LogInformation($"Начата обработка документа по пути: {state.PathToFile(chatId)}");
        await ProcessFile(state.PathToFile(chatId), client, chatId, cancellationToken);
    }

    /// <summary>
    /// Чтение файлов подходящего формата.
    /// </summary>
    /// <param name="filePath">Путь до файла.</param>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <returns>Массив объектов станций.</returns>
    private async Task<MetroStation[]> ProcessFile(string filePath, ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken)
    {
        MetroStation[] stations;
        
        // Вызов различнных методов для чтения в зависимости от расширения файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начато чтение csv-документа пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing();
            using var streamReader = new StreamReader(filePath);
            stations = await csvProcessing.Read(streamReader, client, chatId, cancellationToken);
            _logger.LogInformation($"Окончено чтение csv-документа пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начато чтение json-документа пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            using var streamReader = new StreamReader(filePath);
            stations = await jsonProcessing.Read(streamReader, client, chatId, cancellationToken);
            _logger.LogInformation($"Окончено чтение json-документа пользователя с id: {chatId}");
        }

        return stations;
    }

    /// <summary>
    /// Отправка файла пользователю.
    /// </summary>
    /// <param name="stream">Поток с файлом для отправки.</param>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task UploadFile(FileStream stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
        // Создание потока по документации.
        await using var streamWriter = new StreamWriter(stream);
        var state = new State();
        _logger.LogInformation($"Начата выгрузка документа по пути: {state.PathToFile(chatId)}" +
                               $" пользователя с id: {chatId}");
        
        // Отправка документа в чат.
        await client.SendDocumentAsync(
            chatId: chatId,
            document: InputFile.FromStream(stream: stream, fileName: Path.GetFileName(state.PathToFile(chatId))),
            caption: "Возвращаю обработанный файл!", cancellationToken: cancellationToken);
        _logger.LogInformation($"Окончена выгрузка документа по пути: {state.PathToFile(chatId)}" +
                               $" пользователя с id: {chatId}");
    }

    /// <summary>
    /// Обработчик команды /start.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task HandleCommandStart(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Обработка команды /start пользователя с id: {chatId}");
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
    
    /// <summary>
    /// Обработчик команды /help.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task HandleCommandHelp(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Обработка команды /help пользователя с id: {chatId}");
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
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Тссс! Хочешь Секрет?\nЕсли жмыкнуть /files, то бот сам предоставит красивые файлики " +
                  "с пригодными данными!\nИ даже не надо пользоваться неприятными данными из ТЗ!",
            cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Обработчик команды /work.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task HandleCommandWork(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Обработка команды /work пользователя с id: {chatId}");
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
    
    /// <summary>
    /// Обработчик команды /files.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены потоков.</param>
    private async Task HandleCommandFiles(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Обработка команды /files пользователя с id: {chatId}");
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELxHBl_GiCuKgR6zl5riwGO1hU1AZpuQAC-xUAAqJi8UiWlKoSmWPXjjQE"),
            cancellationToken: cancellationToken);

        using (FileStream stream = new FileStream("moscow_metro.json", FileMode.Open))
        {
            await client.SendDocumentAsync(
                chatId: chatId,
                document: InputFile.FromStream(stream: stream, fileName: "moscow_metro.json"),
                cancellationToken: cancellationToken);
        }
        using (FileStream stream = new FileStream("moscow_metro.csv", FileMode.Open))
        {
            await client.SendDocumentAsync(
                chatId: chatId,
                document: InputFile.FromStream(stream: stream, fileName: "moscow_metro.csv"),
                cancellationToken: cancellationToken);
        }
    }
    
    /// <summary>
    /// Обработчик сортировки по году.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="stations">Массив объектов-станций.</param>
    /// <param name="state">Объект стейтов.</param>
    private async Task HandleCallbackSortYear(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state)
    {
        // Получение отсортированного массива станций.
        var dataTool = new DataTool();
        stations = dataTool.SortByYear(stations);
        _logger.LogInformation($"Получен отсортированный по году документ пользователя с id: {chatId}");
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        // Запись в файл в зависимости от разрешения входного файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начата запись в csv-документ пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в csv-документ пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начата запись в json-документ пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в json-документ пользователя с id: {chatId}");
        }
        
        // Выгрузка файла пользователю.
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    /// <summary>
    /// Обработчик сортировки по названию станции.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="stations">Массив объектов-станций.</param>
    /// <param name="state">Объект стейта.</param>
    private async Task HandleCallbackSortName(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state)
    {
        // Получение отсортированного массива станций.
        var dataTool = new DataTool();
        stations = dataTool.SortByName(stations);
        _logger.LogInformation($"Получен отсортированный по названию документ пользователя с id: {chatId}");
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        // Запись в файл в зависимости от разрешения исходного файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начата запись в csv-документ пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в csv-документ пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начата запись в json-документ пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в json-документ пользователя с id: {chatId}");
        }
        
        // Выгрузка файла пользователю.
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    /// <summary>
    /// Обработчик фильтрации по названию станции.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="stations">Массив станций.</param>
    /// <param name="state">Объект стейта.</param>
    /// <param name="filterValue">Значение для фильтрации.</param>
    private async Task HandleCallbackFilterName(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterValue)
    {
        // Получение отфильтрованного массива станций.
        var dataTool = new DataTool();
        stations = dataTool.Filter(stations, filterValue, "name");
        _logger.LogInformation($"Получен отфильтрованный по названию со значением: {filterValue} " +
                               $"документ пользователя с id: {chatId}");
        
        FileStream stream;
        var filePath = state.PathToFile(chatId);

        // Запись в файл в зависимости от расширения исходного файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начата запись в csv-документ пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в csv-документ пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начата запись в json-документ пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в json-документ пользователя с id: {chatId}");
        }
        
        // Выгрузка файла пользователю.
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    /// <summary>
    /// Обработчик фильтрации по названию линии.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="stations">Массив станций.</param>
    /// <param name="state">Объект стейта.</param>
    /// <param name="filterValue">Значение для фильтрации.</param>
    private async Task HandleCallbackFilterLine(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterValue)
    {
        var dataTool = new DataTool();
        var filePath = state.PathToFile(chatId);

        // Получение отфильтрованного массива станций.
        FileStream stream;
        stations = dataTool.Filter(stations, filterValue, "line");
        _logger.LogInformation($"Получен отфильтрованный по линии со значением: {filterValue} " +
                               $"документ пользователя с id: {chatId}");

        // Запись в файл в зависимости от расширения исходного файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начата запись в csv-документ пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в csv-документ пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начата запись в json-документ пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в json-документ пользователя с id: {chatId}");
        }
        
        // Выгрузка файла пользователю.
        await UploadFile(stream, client, chatId, cancellationToken);
    }

    /// <summary>
    /// Обработчик фильтрации по обоим параметрам.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <param name="stations">Массив станций.</param>
    /// <param name="state">Объект стейта.</param>
    /// <param name="filterName">Фильтр для названия станции.</param>
    /// <param name="filterMonth">Фильтр названия месяца.</param>
    private async Task HandleCallbackFilterBoth(ITelegramBotClient client, long chatId, 
        CancellationToken cancellationToken, MetroStation[] stations, State state, string filterName, string filterMonth)
    {
        var dataTool = new DataTool();
        var filePath = state.PathToFile(chatId);

        // Получение отфильтрованного массива станций.
        FileStream stream;
        stations = dataTool.FilterByTwoFields(stations, filterName, filterMonth);
        _logger.LogInformation($"Получен отфильтрованный по обоим полям со значениями: {filterName}/{filterMonth} " +
                               $"документ пользователя с id: {chatId}");

        // Запись в файл в зависимости от расширения исходного файла.
        if (Path.GetExtension(filePath) == ".csv")
        {
            _logger.LogInformation($"Начата запись в csv-документ пользователя с id: {chatId}");
            var csvProcessing = new CsvProcessing(); 
            stream = await csvProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в csv-документ пользователя с id: {chatId}");
        }
        else
        {
            _logger.LogInformation($"Начата запись в json-документ пользователя с id: {chatId}");
            var jsonProcessing = new JsonProcessing();
            stream = await jsonProcessing.Write(stations, state.PathToFile(chatId));
            _logger.LogInformation($"Окончена запись в json-документ пользователя с id: {chatId}");
        }
        
        // Выгрузка файла пользователю.
        await UploadFile(stream, client, chatId, cancellationToken);
    }
    
    /// <summary>
    /// Сообщение пользователю о том, что данные загружены.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task SayFileDownloaded(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Предложение выбрать действие из меню для пользователя с id: {chatId}");
        await client.SendStickerAsync(
            chatId: chatId,
            sticker: InputFile.FromFileId(
                "CAACAgIAAxkBAAELu5dl9y-bmYP3Zg5lLyz-GpiBP6-fOAACERgAAv564UuysbzruCnEmzQE"),
            cancellationToken: cancellationToken);

        // Создание инлайн кнопок.
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

    /// <summary>
    /// Сообщение пользователю о том, что нужно ввести /help, если он что-то забыл.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task SayToEnterCommand(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Сообщение о том, что введено неверное значение для пользователя с id: {chatId}");
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
    
    /// <summary>
    /// Сообщение пользователю о том, что нужно ввести значения фильтров.
    /// </summary>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    private async Task SayAboutRightValues(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Сообщение о том, что нужно ввести значения для фильтров" +
                               $" для пользователя с id: {chatId}");
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