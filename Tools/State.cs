namespace Tools;

/// <summary>
/// Класс для хранения стейтов (состояний) каждого пользователя.
/// Нужен для корректной обработки информации при наличии большой нагрузки на бота.
/// </summary>
public class State
{
    // Путь к файлу со стейтами.
    private static readonly char Separator = Path.DirectorySeparatorChar;
    private readonly string _systemFile =
            $"..{Separator}..{Separator}..{Separator}..{Separator}WorkingFiles{Separator}system{Separator}users.txt";
    
    /// <summary>
    /// Метод для добавления пользователя в файл.
    /// </summary>
    /// <param name="chatId">ID пользователя для создания уникального имени файла.</param>
    public void AddUser(long chatId)
    {
        // Чтение информации из файла.
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }
        
        // Если пользователь с таким ID не нашелся, добавляем его в файл.
        if (!lines.Any(line => line.Contains(chatId.ToString())))
        {
            lines.Add($"{chatId}: pass: false");
        }

        // Запись обратно в файл.
        using (var streamWriter = new StreamWriter(_systemFile))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }

    /// <summary>
    /// Метод для добавления пути до входного файла к конкретному пользователю.
    /// </summary>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="filePath">Путь к входному файлу.</param>
    public void AddFileToUser(long chatId, string filePath)
    {
        // Чтение файла.
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }
        
        // Ищем пользователя с таким ID и перезаписываем информацию о файле для него.
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            var values = line.Split(": ");
            lines[Array.IndexOf(lines.ToArray(), line)] = $"{values[0]}: {filePath}: {values[2]}";
            break;
        }

        // Запись обратно в файл.
        using (var streamWriter = new StreamWriter(_systemFile))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }

    /// <summary>
    /// Добавление стейта для пользователя.
    /// </summary>
    /// <param name="chatId">ID пользователя.</param>
    /// <param name="state">Значение стейта фильтрации.</param>
    public void AddStateToUser(long chatId, string state)
    {
        // Считывание информации из файла.
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }
        
        // Ищем пользователя с таким ID и перезаписываем ему стейт.
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            var values = line.Split(": ");
            lines[Array.IndexOf(lines.ToArray(), line)] = $"{values[0]}: {values[1]}: {state}";
            break;
        }

        // Запись обратно в файл.
        using (var streamWriter = new StreamWriter(_systemFile))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }
    
    /// <summary>
    /// Получение пути до входного файла для конкретного пользователя.
    /// </summary>
    /// <param name="chatId">ID пользователя.</param>
    /// <returns>Путь до входного файла.</returns>
    public string PathToFile(long chatId)
    {
        // Считывание информации из файла.
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }

        // Поиск пользователя с нужным ID в файле и запись полученного пути в переменную.
        var filePath = String.Empty;
        
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            filePath = line.Split(": ")[1];
        }

        return filePath;
    }
    
    /// <summary>
    /// Метод для получения метода фильтрации (стейта) пользователя.
    /// </summary>
    /// <param name="chatId">ID пользователя.</param>
    /// <returns>Стейт фильтрации пользователя.</returns>
    public string UserState(long chatId)
    {
        // Считывание информации из файла.
        var lines = new List<string>();
        
        using (var streamReader = new StreamReader(_systemFile))
        {
            while (streamReader.ReadLine() is { } line)
            {
                lines.Add(line);
            }
        }

        // Поиск пользователя с нужным ID и запись его стейта в переменную.
        var state = String.Empty;
        
        foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
        {
            state = line.Split(": ")[2];
        }

        return state;
    }
}