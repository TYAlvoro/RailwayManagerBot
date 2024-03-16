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
        var separator = Path.DirectorySeparatorChar;
        var filePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output{separator}{fileName}";

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };

        await using (var writer = new StreamWriter(filePath))
        {
            await using (var csv = new CsvWriter(writer, config))
            {
                await csv.WriteRecordsAsync(stations.ToList());
            }
        }
        
        return new FileStream(filePath, FileMode.Open);
    }
}