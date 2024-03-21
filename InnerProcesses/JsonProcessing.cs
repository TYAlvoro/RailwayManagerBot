using Newtonsoft.Json;
using Telegram.Bot;

namespace InnerProcesses;

/// <summary>
/// Класс, реализующий работу с JSON файлами.
/// </summary>
public class JsonProcessing
{
    /// <summary>
    /// Метод, позволяющий читать JSON файлы.
    /// </summary>
    /// <param name="stream">Поток с входным файлом.</param>
    /// <param name="client">Бот-клиент.</param>
    /// <param name="chatId">ID чата, в котором происходит взаимодействие.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <returns>Массив объектов станций.</returns>
    public async Task<MetroStation[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
        string jsonString;
        
        // Считывание информации в одну строку.
        using (var streamReader = stream)
        {
            jsonString = await streamReader.ReadToEndAsync();
        }

        var stations = new List<MetroStation>();

        // Десериализация JSON файла.
        try
        {
            stations = JsonConvert.DeserializeObject<List<MetroStation>>(jsonString);
        }
        catch (JsonException ex)
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: $"Ошибка! В файле обнаружены некорректные данные!\n{ex.Message}",
                cancellationToken: cancellationToken);
        }

        return stations!.ToArray();
    }

    /// <summary>
    /// Метод, реализующий запись в JSON файл.
    /// </summary>
    /// <param name="stations">Массив объектов для записи.</param>
    /// <param name="fileName">Путь до выходного файла.</param>
    /// <returns>Поток для записи из бот-клиента.</returns>
    public async Task<FileStream> Write(MetroStation[] stations, string fileName)
    {
        // Получение пути к файлу в системе.
        var separator = Path.DirectorySeparatorChar;
        var filePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}" +
            $"output{separator}{Path.GetFileName(fileName)}";
        
        // Безопасная запись в файл.
        await using (var streamWriter = new StreamWriter(filePath))
        {
            var jsonString = JsonConvert.SerializeObject(stations, Formatting.Indented);
            await streamWriter.WriteAsync(jsonString);
        }
        
        return new FileStream(filePath, FileMode.Open);
    }
}