using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Telegram.Bot;
using Tools;

namespace InnerProcesses;

public class CsvProcessing
{
    public CsvProcessing() { }

    public async Task Read(StreamReader stream, ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };
        
        using var reader = stream;
        using var csv = new CsvReader(reader, config);

        try
        {
            csv.Read();
            csv.ReadHeader();
            csv.Read();
            
            var records = csv.GetRecords<MetroStation>().ToList();

            foreach (var station in records)
            {
                Console.WriteLine($"ID: {station.Id}, Name: {station.NameOfStation}");
            }
        }
        catch (CsvHelperException ex)
        {
            Typewriter typewriter = new Typewriter();
            string message =
                $"Ошибка! В файле обнаружены некорректные данные!\nСтрока: {ex.Context.Parser.Row}\n" +
                $"Текст строки (может отсутствовать в зависимости от ваших данных): {ex.Context.Parser.Record}";
            await typewriter.TypeMessageByWords(client, chatId, cancellationToken, message);
        }
    } 
}