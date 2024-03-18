namespace Tools;

public class State
{
    private static readonly char Separator = Path.DirectorySeparatorChar;
    private string _systemFile =
            $"..{Separator}..{Separator}..{Separator}..{Separator}WorkingFiles{Separator}system{Separator}users.txt";
    
    public void AddUser(long chatId)
    {
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }
        
        if (!lines.Any(line => line.Contains(chatId.ToString())))
        {
            lines.Add($"{chatId}: pass");
        }

        using (var streamWriter = new StreamWriter(_systemFile))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }

    public void AddFileToUser(long chatId, string filePath)
    {
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }
        
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            lines[Array.IndexOf(lines.ToArray(), line)] = $"{line.Split(": ")[0]}: {filePath}";
            break;
        }

        using (var streamWriter = new StreamWriter(_systemFile))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }
}