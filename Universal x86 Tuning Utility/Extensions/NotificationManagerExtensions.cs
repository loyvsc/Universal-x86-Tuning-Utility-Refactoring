using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopNotifications;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class NotificationManagerExtensions
{
    public static readonly DateTimeOffset DefaultNotificationExpirationTime = new(0, 0, 0, 0, 0, 3, TimeSpan.Zero);
    
    public static readonly DateTimeOffset ErrorNotificationExpirationTime = new(0, 0, 0, 0, 0, 3, TimeSpan.Zero);

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
            NotificationType.Normal => DefaultNotificationExpirationTime,
            NotificationType.Error => ErrorNotificationExpirationTime
        };
        
        await notificationManager.ShowNotification(notification, expirationTime);
    }
}