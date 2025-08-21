using Microsoft.Extensions.Logging;

namespace FileCloud.Desktop.Services.Services
{
    public static class ServerStateService
    {
        public static bool IsServerActive { get; private set; } = false;

        public static void SetServerState(bool state)
        {
            IsServerActive = state;
        }

        public static async Task<T> ExecuteIfServerActive<T>(ILogger logger, Func<Task<T>> action)
        {
            if (!IsServerActive)
            {
                logger.LogWarning("Сервер закрыт, пропуск запроса.");
                throw new InvalidOperationException("Сервер закрыт.");
            }

            return await action();
        }

        public static async Task ExecuteIfServerActive(ILogger logger, Func<Task> action)
        {
            if (!IsServerActive)
            {
                logger.LogWarning("Сервер закрыт, пропуск запроса.");
                throw new InvalidOperationException("Сервер закрыт.");
            }

            await action();
        }
    }
}
