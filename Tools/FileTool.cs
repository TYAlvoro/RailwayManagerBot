namespace Tools;

public class FileTool
{
    public void CreateDirectories()
    {
        char separator = Path.DirectorySeparatorChar;
        string mainDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles";
        string inputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}input";
        string outputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output";

        try
        {
            if (!Directory.Exists(mainDirectory))
            {
                Directory.CreateDirectory(mainDirectory);
                Console.WriteLine("\"Рядом\" с папками проекта создана папка WorkingFiles для хранения файлов работы.");
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
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Программа не имеет разрешения на создание нужной для ее корректной работы папки!: {ex}");
        }
    }
}