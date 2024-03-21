using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Telegram.Bot;

namespace InnerProcesses;

/// <summary>
/// Класс для работы с CSV файлами.
/// </summary>
public class CsvProcessing
{
    /// <summary>
    /// Метод, позволяющий читать CSV файлы.
    /// </summary>
    /// <param name="stream">Поток с файлом для чтения.</param>
    /// <param name="client">Клиент бота.</param>
    /// <param name="chatId">Id чата, в котором происходит взаимодействие.</param>
    /// <param name="cancellationToken">Токен отмены для потоков.</param>
    /// <returns>Массив объектов станций метро.</returns>
    public async Task<MetroStation[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
        // Создание конфигурации чтения файла. 
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };
        
        using var reader = stream;
        using var csv = new CsvReader(reader, config);

        List<MetroStation> stations = new List<MetroStation>();

        try
        {
            // Считывание и игнорирование двух заголовков для корректной обработки всего файла.
            await csv.ReadAsync();
            csv.ReadHeader();
            await csv.ReadAsync();
            
            // Десериализация csv файла в массив объектов-станций.
            stations = csv.GetRecords<MetroStation>().ToList();
        }
        catch (CsvHelperException ex)
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: $"Ошибка! В файле обнаружены некорректные данные!\nСтрока: {ex.Context.Parser.Row}\n" +
                      $"Текст строки (может отсутствовать в зависимости от ваших данных): {ex.Context.Parser.RawRecord}",
                cancellationToken: cancellationToken);
        }

        return stations.ToArray();
    }

    /// <summary>
    /// Метод для записи CSV информации в файл.
    /// </summary>
    /// <param name="stations">МАссив объектов для сериализации.</param>
    /// <param name="fileName">Путь до выходного файла (путь берется из клиента телеграмма).</param>
    /// <returns>Поток для использования в методе записи бот-клиента.</returns>
    public async Task<FileStream> Write(MetroStation[] stations, string fileName)
    {
        // Получение пути до файла в системе.
        char separator = Path.DirectorySeparatorChar;
        string filePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}" +
            $"output{separator}{Path.GetFileName(fileName)}";
        
        // Задание верной конфигурации для csv файла.
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false
        };

        await using (var streamWriter = new StreamWriter(filePath))
        {
            await using (var csv = new CsvWriter(streamWriter, config))
            {
                // Запись двух заголовков.
                streamWriter.WriteLine("\"ID\";\"NameOfStation\";\"Line\";\"Longitude_WGS84\";\"Latitude_WGS84\";" +
                                       "\"AdmArea\";\"District\";\"Year\";\"Month\";\"global_id\";\"geodata_center\";" +
                                       "\"geoarea\";");
                streamWriter.WriteLine("\"\u2116 п/п\";\"Станция метрополитена\";\"Линия\";\"Долгота в WGS-84\";" +
                                       "\"Широта в WGS-84\";\"Административный округ\";\"Район\";\"Год\";\"Месяц\";" +
                                       "\"global_id\";\"geodata_center\";\"geoarea\";");
                // Запись самих станций.
                await csv.WriteRecordsAsync(stations.ToList());
            }
        }
        
        return new FileStream(filePath, FileMode.Open);
    }
}