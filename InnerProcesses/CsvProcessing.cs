using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Telegram.Bot;

namespace InnerProcesses;

public class CsvProcessing
{
    public CsvProcessing() { }

    public async Task<MetroStation[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
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
            await csv.ReadAsync();
            csv.ReadHeader();
            await csv.ReadAsync();
            
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

    public async Task<FileStream> Write(MetroStation[] stations, string fileName)
    {
        char separator = Path.DirectorySeparatorChar;
        string filePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output{separator}{Path.GetFileName(fileName)}";
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false
        };

        await using (var streamWriter = new StreamWriter(filePath))
        {
            await using (var csv = new CsvWriter(streamWriter, config))
            {
                streamWriter.WriteLine("\"ID\";\"NameOfStation\";\"Line\";\"Longitude_WGS84\";\"Latitude_WGS84\";" +
                                       "\"AdmArea\";\"District\";\"Year\";\"Month\";\"global_id\";\"geodata_center\";" +
                                       "\"geoarea\";");
                streamWriter.WriteLine("\"\u2116 п/п\";\"Станция метрополитена\";\"Линия\";\"Долгота в WGS-84\";" +
                                       "\"Широта в WGS-84\";\"Административный округ\";\"Район\";\"Год\";\"Месяц\";" +
                                       "\"global_id\";\"geodata_center\";\"geoarea\";");
                await csv.WriteRecordsAsync(stations.ToList());
            }
        }
        
        return new FileStream(filePath, FileMode.Open);
    }
}