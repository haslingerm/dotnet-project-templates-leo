using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace LeoGRpcApi.Client.Util;

/// <summary>
///     A service for showing toast notifications in the application
/// </summary>
public interface IToastService
{
    /// <summary>
    ///     Shows a toast notification with the specified message, title, and type.
    ///     The window to display in must be set before calling this method, see <see cref="SetWindow" />.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">The title of the notification</param>
    /// <param name="type">The notification type - determines e.g. coloring</param>
    public void ShowMessage(string message, string title = "Information",
                            NotificationType type = NotificationType.Information);

    /// <summary>
    ///     Sets the window to display notifications in.
    ///     In this application, that is always the main window, so this method should not be called outside the setup phase.
    /// </summary>
    /// <param name="window">The window to show the notifications in</param>
    public void SetWindow(Window window);
}

internal sealed class ToastService(ILogger<ToastService> logger) : IToastService
{
    private WindowNotificationManager? _notificationManager;

    public void ShowMessage(string message, string title, NotificationType type)
    {
        if (_notificationManager is null)
        {
            logger.LogWarning("Notification manager is not set. Call SetWindow() before showing messages.");

            return;
        }

        var notification = new Notification(title, message, type);
        _notificationManager.Show(notification);
    }

    public void SetWindow(Window window)
    {
        _notificationManager = new WindowNotificationManager(window)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 5
        };
    }
}
