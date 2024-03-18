namespace Tools;

public class FileTool
{
    public void CreateDirectories()
    {
        char separator = Path.DirectorySeparatorChar;
        string mainDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles";
        string inputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}input";
        string outputDirectory = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}output";
        string systemFiles = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}system";

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

    public void CreateFile()
    {
        char separator = Path.DirectorySeparatorChar;
        string systemFile =
            $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}system{separator}users.txt";

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