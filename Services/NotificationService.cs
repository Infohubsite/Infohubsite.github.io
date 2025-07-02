using MudBlazor;

namespace Frontend.Services
{
    public interface INotificationService
    {
        event Func<string, Severity, Task>? OnShow;
        Task Show(string message, Severity severity = Severity.Error);
    }

    public class NotificationService : INotificationService
    {
        public event Func<string, Severity, Task>? OnShow;

        public async Task Show(string message, Severity severity)
        {
            if (OnShow != null)
            {
                await OnShow.Invoke(message, severity);
            }
        }
    }
}