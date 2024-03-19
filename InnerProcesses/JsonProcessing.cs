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

        var stations = new List<MetroStation>();

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
        var separator = Path.DirectorySeparatorChar;
        var filePath =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output{separator}{Path.GetFileName(fileName)}";
        
        await using (var streamWriter = new StreamWriter(filePath))
        {
            var jsonString = JsonConvert.SerializeObject(stations, Formatting.Indented);
            await streamWriter.WriteAsync(jsonString);
        }
        
        return new FileStream(filePath, FileMode.Open);
    }
}