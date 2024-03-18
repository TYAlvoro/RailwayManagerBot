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
            lines.Add($"{chatId}: pass: false");
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

    public void AddStateToUser(long chatId, string filePath, string state)
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
            var values = line.Split(": ");
            lines[Array.IndexOf(lines.ToArray(), line)] = $"{values[0]}: {values[1]}: {state}";
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
    
    public void AddValuesToUser(long chatId, string filePath, string firstValue, string secondValue)
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
            var values = line.Split(": ");
            lines[Array.IndexOf(lines.ToArray(), line)] = 
                $"{values[0]}: {values[1]}: {values[2]}: {firstValue}: {secondValue}";
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
    
    public string PathToFile(long chatId)
    {
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }

        var filePath = String.Empty;
        
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            filePath = line.Split(": ")[1];
        }

        return filePath!;
    }
}