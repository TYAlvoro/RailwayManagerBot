using Microsoft.Extensions.Logging;

namespace Tools
{
    /// <summary>
    /// Класс, представляющий логгер для записи в файл.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly object _lockObj = new ();

        /// <summary>
        /// Создает новый экземпляр класса FileLogger с указанным путем к файлу лога.
        /// </summary>
        /// <param name="filePath">Путь к файлу лога.</param>
        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Начинает новую область логирования. В данной реализации не используется.
        /// </summary>
        /// <typeparam name="TState">Тип состояния.</typeparam>
        /// <param name="state">Состояние.</param>
        /// <returns>Возвращает null.</returns>
        public IDisposable? BeginScope<TState>(TState state) => null;

        /// <summary>
        /// Проверяет, включен ли указанный уровень логирования.
        /// </summary>
        /// <param name="logLevel">Уровень логирования для проверки.</param>
        /// <returns>Возвращает true, если указанный уровень логирования включен, иначе false.</returns>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Записывает сообщение в файл лога.
        /// </summary>
        /// <typeparam name="TState">Тип состояния.</typeparam>
        /// <param name="logLevel">Уровень логирования.</param>
        /// <param name="eventId">Идентификатор события.</param>
        /// <param name="state">Состояние.</param>
        /// <param name="exception">Исключение (если есть).</param>
        /// <param name="formatter">Функция форматирования сообщения.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
            Func<TState, Exception, string> formatter)
        {
            // Проверяем, включено ли логирование на указанном уровне
            if (!IsEnabled(logLevel))
                return;

            // Блокируем объект для синхронизации доступа к файлу лога
            lock (_lockObj)
            {
                try
                {
                    // Записываем сообщение в файл лога
                    using (var writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now} [{logLevel}] - {formatter(state, exception)}");
                    }
                }
                catch (Exception ex)
                {
                    // В случае ошибки выводим сообщение в консоль
                    Console.WriteLine($"Ошибка при записи в лог: {ex.Message}");
                }
            }
        }
    }
}