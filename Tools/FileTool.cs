namespace Tools;

/// <summary>
/// Класс для работы с директориями и файлами.
/// </summary>
public class FileTool
{
    /// <summary>
    /// Создание нужных для работы директорий.
    /// </summary>
    public void CreateDirectories()
    {
        // Инициализация путей до директорий.
        var separator = Path.DirectorySeparatorChar;
        var mainDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles";
        var inputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}input";
        var outputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output";
        var systemFiles = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}system";
        var varDirectory = $"..{separator}..{separator}..{separator}..{separator}var";

        // Попытка создания директорий.
        try
        {
            if (!Directory.Exists(mainDirectory))
            {
                Directory.CreateDirectory(mainDirectory);
                Console.WriteLine("\"Рядом\" с папками проекта создана папка WorkingFiles для хранения файлов работы.");
            }
            if (!Directory.Exists(varDirectory))
            {
                Directory.CreateDirectory(varDirectory);
                Console.WriteLine("\"Рядом\" с папками проекта создана папка var для хранения файлов логов.");
            }
            if (!Directory.Exists(inputDirectory))
            {
                Directory.CreateDirectory(inputDirectory);
                Console.WriteLine("В папке WorkingFiles создана папка input для хранения входных файлов.");
            }
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                Console.WriteLine("В папке WorkingFiles создана папка output для хранения выходных файлов.");
            }
            if (!Directory.Exists(systemFiles))
            {
                Directory.CreateDirectory(systemFiles);
                Console.WriteLine("В папке WorkingFiles создана папка system для хранения файлов программы.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Программа не имеет разрешения на создание нужной для ее корректной работы папки!: {ex}");
        }
    }
    
    /// <summary>
    /// Создание файла для стейтов.
    /// </summary>
    public void CreateStateFile()
    {
        // Инициализация пути до файла.
        var separator = Path.DirectorySeparatorChar;
        var systemFile =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}system{separator}users.txt";

        // Попытка создания файла.
        try
        {
            if (File.Exists(systemFile)) return;
            using (var _ = File.Create(systemFile))
            {
                Console.WriteLine("В папке system создан файл users.txt для хранения состояний.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Программа не имеет разрешения на создание нужного для ее корректной работы файла!: {ex}");
        }
    }
}