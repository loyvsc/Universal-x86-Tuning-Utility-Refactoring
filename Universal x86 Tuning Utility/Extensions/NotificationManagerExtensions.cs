using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopNotifications;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class NotificationManagerExtensions
{
    private static readonly TimeSpan DefaultNotificationExpirationTimeSpan = TimeSpan.FromSeconds(3);
    
    private static readonly TimeSpan ErrorNotificationExpirationTimeSpan = TimeSpan.FromSeconds(4);

    public enum NotificationType
    {
        Normal,
        Error
    }
    
    public static async Task ShowTextNotification(this INotificationManager notificationManager, string title,
                                              string text,
                                              NotificationType notificationType = NotificationType.Normal,
                                              CancellationToken cancellationToken = default)
    {
        var notification = new Notification()
        {
            Title = title,
            Body = text
        };

        var expirationTime = notificationType switch
        {
            NotificationType.Normal => DefaultNotificationExpirationTimeSpan,
            NotificationType.Error => ErrorNotificationExpirationTimeSpan
        };
        
        await notificationManager.ShowNotification(notification, DateTimeOffset.Now + expirationTime);
    }
}