using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;

namespace InnerProcesses;

public class JsonProcessing
{
    public JsonProcessing() { }

    public async Task<MetroStation[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
    {
        string jsonString;
        using (var _ = stream)
        {
            jsonString = await stream.ReadToEndAsync();
        }

        List<MetroStation>? stations = new List<MetroStation>();

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

    public async Task<FileStream> Write(MetroStation[] stations, string fileName)
    {
        await using (var streamWriter = new StreamWriter(fileName))
        {
            var jsonString = JsonConvert.SerializeObject(stations, Formatting.Indented);
            await streamWriter.WriteAsync(jsonString);
        }
        
        return new FileStream(fileName, FileMode.Open);
    }
}